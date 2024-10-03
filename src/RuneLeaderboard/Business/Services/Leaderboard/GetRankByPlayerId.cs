using Api.Business.Results;
using Api.Data.Postgres.Models;
using Api.Data.Redis;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

namespace Api.Business.Services.Leaderboard;

public abstract class GetRankByPlayerId
{
    public class GetRankByPlayerIdRequestHandler
    {
        private readonly ConfigurationOptions.PostgresSqlOptions _postgreSqlOptions;
        private readonly RedisLeaderboardService _redisLeaderboardService;

        public GetRankByPlayerIdRequestHandler(
            IOptions<ConfigurationOptions.PostgresSqlOptions> postgreSqlOptions,
            RedisLeaderboardService redisLeaderboardService)
        {
            _postgreSqlOptions = postgreSqlOptions.Value;
            _redisLeaderboardService = redisLeaderboardService;
        }

        public async Task<DataResult<LeaderboardRanking>> HandleAsync(int playerId)
        {
            try
            {
                var (ranking, redisError) = await _redisLeaderboardService.GetRankingByPlayerId(playerId);

                if (ranking == null || !string.IsNullOrEmpty(redisError))
                {
                    using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

                    var sql = @"
                                     SELECT * 
                                     FROM ""LeaderboardRankingsMat"" AS ""l""
                                     JOIN ""Players"" AS ""p""
                                     ON ""l"".""PlayerId"" = ""p"".""Id""
                                     JOIN ""LeaderboardScores"" AS ""s""
                                     ON ""l"".""PlayerId"" = ""s"".""PlayerId""
                                     WHERE ""l"".""PlayerId"" = @PlayerId";

                    ranking = await connection.QueryFirstOrDefaultAsync<LeaderboardRanking>(sql, new { PlayerId = playerId });
                }

                if (ranking == null)
                    return new DataResult<LeaderboardRanking>(message: "Player ranking not found", status: ResultStatus.NotFound);

                return new DataResult<LeaderboardRanking>(data: ranking);
            }
            catch (Exception ex)
            {
                Log.Fatal("{message} : {stackTrace}", ex.Message, ex.StackTrace);

                return DataResult<LeaderboardRanking>.Error();
            }
        }
    }
}

