using Api.Business.Results;
using Api.Business.Utilities.Security.Encryption;
using Api.Constants;
using Api.Data.Postgres.Models;
using Api.Data.Redis;
using Dapper;
using Hangfire;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using Serilog;

namespace Api.Business.Services.Leaderboard;

public abstract class MatchResultEncryptedService
{
    public class MatchResultEncryptedRequest
    {
        public string Data { get; set; } = default!;
        public string IV { get; set; } = default!;
    }

    public class MatchResultRequestModel
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

        public async Task<DataResult<MatchResultResponse>> HandleAsync(MatchResultEncryptedRequest request, int playerId)
        {
            try
            {
                string? decryptedData = null;
                MatchResultRequestModel? decryptedDetails = null;

                try
                {
                    decryptedData = AesEncryption.Decrypt(request.Data, _encryptionOptions.AesKey, request.IV);
                    decryptedDetails = JsonConvert.DeserializeObject<MatchResultRequestModel>(decryptedData);
                }
                catch
                {
                    return new DataResult<MatchResultResponse>(message: Messages.RequestInvalid, status: ResultStatus.RequestInvalid);
                }

                if (decryptedDetails!.Score <= 0)
                    return new DataResult<MatchResultResponse>(message: Messages.RequestInvalid, status: ResultStatus.RequestInvalid);

                using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

                var sql = @"SELECT * FROM ""LeaderboardScores"" WHERE ""PlayerId"" = @PlayerId";

                var score = await connection.QueryFirstOrDefaultAsync<LeaderboardScore>(sql, new { PlayerId = playerId });

                if (score == null)
                {
                    sql = @"INSERT INTO ""LeaderboardScores"" (""PlayerId"", ""Score"", ) VALUES(@PlayerId, @Score)";

                    await connection.ExecuteAsync(sql, new { PlayerId = playerId, Score = decryptedDetails.Score });
                }
                else if (score.Score < decryptedDetails.Score)
                {
                    sql = @"UPDATE ""LeaderboardScores"" SET ""Score"" = @Score, ""CreatedAt"" = @CreatedAt, WHERE ""PlayerId"" = @PlayerId";

                    await connection.ExecuteAsync(sql, new { Score = decryptedDetails.Score, PlayerId = playerId });
                }

                sql = @"SELECT PlayerId, Username, Score, RANK FROM ""LeaderboardRankingsView"" WHERE PlayerId = @PlayerId";

                var ranking = await connection.QuerySingleOrDefaultAsync<LeaderboardRanking>(sql, new { PlayerId = playerId });

                if (ranking == null)
                {
                    Log.Error("Cant retrieve rank from view playerId => {}", playerId);

                    return new DataResult<MatchResultResponse>(message: Messages.UnexpectedError, status: ResultStatus.Error);
                }

                if (ranking.Rank <= RedisConstants.TopRankCount)
                {
                    BackgroundJob.Enqueue(() => _redisLeaderboardService.RefreshTopRanks());
                }

                return new DataResult<MatchResultResponse>(message: Messages.Success, data: new MatchResultResponse
                {
                    Rank = ranking.PlayerId,
                });
            }
            catch (Exception ex)
            {
                Log.Fatal("{message} :  {stackTrace}", ex.Message, ex.StackTrace);

                return new DataResult<MatchResultResponse>(message: Messages.UnexpectedError, status: ResultStatus.Error);
            }
        }
    }
}
