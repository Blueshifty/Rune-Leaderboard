using Api.Business.Results;
using Api.Constants;
using Api.Data.Postgres.Models;
using Api.Data.Redis;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

namespace Api.Business.Services.Leaderboard
{
    public abstract class GetRankByRangeService
    {
        public class GetRankByRangeRequest
        {
            public int From { get; set; } = default!;
            public int To { get; set; } = default!;
        }

        public class GetRankByRangeRequestHandler
        {
            private readonly RedisLeaderboardService _redisLeaderboardService;
            private readonly ConfigurationOptions.PostgresSqlOptions _postgreSqlOptions;


            public GetRankByRangeRequestHandler(RedisLeaderboardService redisLeaderboardService,
                IOptions<ConfigurationOptions.PostgresSqlOptions> postgreSqlOptions
                )
            {
                _redisLeaderboardService = redisLeaderboardService;
                _postgreSqlOptions = postgreSqlOptions.Value;
            }

            public async Task<DataResult<List<LeaderboardRanking>>> HandleAsync(GetRankByRangeRequest request)
            {
                try
                {
                    if (request.From < 1 || request.From > request.To)
                        return DataResult<List<LeaderboardRanking>>.InvalidRequest();

                    var rankings = new List<LeaderboardRanking>();

                    string? redisError = null;

                    if (request.To <= RedisConstants.TopRankCount)
                    {
                        (rankings, redisError) = await _redisLeaderboardService.GetRanks(request.From, request.To);
                    }

                    if (request.To > RedisConstants.TopRankCount || !string.IsNullOrEmpty(redisError))
                    {
                        using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

                        var sql = @"
                                     SELECT * 
                                     FROM ""LeaderboardRankingsMat"" AS ""l""
                                     JOIN ""Players"" AS ""p""
                                     ON ""l"".""PlayerId"" = ""p"".""Id""
                                     JOIN ""LeaderboardScores"" AS ""s""
                                     ON ""l"".""PlayerId"" = ""s"".""PlayerId""
                                     WHERE ""l"".""Rank"" >= @From AND ""l"".""Rank"" <= @To
                                     ORDER BY ""l"".""Rank""";

                        rankings = (await connection.QueryAsync<LeaderboardRanking>(sql, new { From = request.From, To = request.To })).ToList();
                    }

                    return new DataResult<List<LeaderboardRanking>>(data: rankings);
                }
                catch (Exception ex)
                {
                    Log.Fatal("{message} : {stackTrace}", ex.Message, ex.StackTrace);

                    return DataResult<List<LeaderboardRanking>>.Error();
                }
            }
        }
    }
}
