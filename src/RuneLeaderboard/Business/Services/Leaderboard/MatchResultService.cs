using Api.Business.Results;
using Api.Constants;
using Api.Data.Postgres.Models;
using Api.Data.Redis;
using Dapper;
using Hangfire;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

namespace Api.Business.Services.Leaderboard
{
    public class MatchResultService
    {
        public class MatchResultRequest
        {
            public int Score { get; set; } = default!;
        }

        public class MatchResultResponse
        {
            public int Rank { get; set; }
        }

        public class MatchResultRequestHandler
        {
            private readonly ConfigurationOptions.PostgresSqlOptions _postgreSqlOptions;
            private readonly ConfigurationOptions.EncryptionOptions _encryptionOptions;
            private readonly RedisLeaderboardService _redisLeaderboardService;

            public MatchResultRequestHandler(
                IOptions<ConfigurationOptions.PostgresSqlOptions> postgreSqlOptions,
                IOptions<ConfigurationOptions.EncryptionOptions> encryptionOptions,
                RedisLeaderboardService redisLeaderboardService)
            {
                _postgreSqlOptions = postgreSqlOptions.Value;
                _encryptionOptions = encryptionOptions.Value;
                _redisLeaderboardService = redisLeaderboardService;
            }

            public async Task<DataResult<MatchResultResponse>> HandleAsync(MatchResultRequest request, int playerId, bool refreshRedis = true)
            {
                try
                {
                    if (request.Score <= 0)
                        return DataResult<MatchResultResponse>.InvalidRequest();

                    using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

                    var sql = @"SELECT * FROM ""LeaderboardScores"" WHERE ""PlayerId"" = @PlayerId";

                    var score = await connection.QueryFirstOrDefaultAsync<LeaderboardScore>(sql, new { PlayerId = playerId });

                    if (score == null)
                    {
                        sql = @"INSERT INTO ""LeaderboardScores"" (""PlayerId"", ""Score"") VALUES(@PlayerId, @Score)";

                        await connection.ExecuteAsync(sql, new { PlayerId = playerId, Score = request.Score });
                    }
                    else if (score.Score < request.Score)
                    {
                        sql = @"UPDATE ""LeaderboardScores"" SET ""Score"" = @Score, ""CreatedAt"" = @CreatedAt WHERE ""PlayerId"" = @PlayerId";

                        await connection.ExecuteAsync(sql, new { Score = request.Score, PlayerId = playerId, CreatedAt = DateTime.UtcNow });
                    }

                    sql = @"SELECT ""PlayerId"", ""Rank"" FROM ""LeaderboardRankingsView"" WHERE ""PlayerId"" = @PlayerId";

                    var ranking = await connection.QuerySingleOrDefaultAsync<LeaderboardRanking>(sql, new { PlayerId = playerId });

                    if (ranking == null)
                    {
                        Log.Error("Cant retrieve rank from view playerId => {}", playerId);

                        return DataResult<MatchResultResponse>.Error();
                    }

                    if (refreshRedis && ranking.Rank <= RedisConstants.TopRankCount)
                    {
                        BackgroundJob.Enqueue(() => _redisLeaderboardService.RefreshTopRanks());
                    }

                    return new DataResult<MatchResultResponse>(message: Messages.Success, data: new MatchResultResponse
                    {
                        Rank = ranking.Rank,
                    });
                }
                catch (Exception ex)
                {
                    Log.Fatal("{message} :  {stackTrace}", ex.Message, ex.StackTrace);

                    return DataResult<MatchResultResponse>.Error();
                }
            }
        }
    }
}
