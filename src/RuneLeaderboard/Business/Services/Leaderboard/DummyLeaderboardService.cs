using Api.Business.Results;
using Api.Business.Services.Auth;
using Api.Data.Postgres.Models;
using Api.Data.Redis;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

namespace Api.Business.Services.Leaderboard
{
    public class DummyLeaderboardService
    {
        public class DummyLeaderboardServiceRequest
        {
            public int PlayerCount { get; set; }
            public int MinScore { get; set; }
            public int MaxScore { get; set; }
        }


        public class DummyLeaderboardServiceRequestHandler
        {
            private readonly ConfigurationOptions.PostgresSqlOptions _postgreSqlOptions;
            private readonly MatchResultService.MatchResultRequestHandler _matchResultServiceRequestHandler;
            private readonly RegisterService.RegisterRequestHandler _registerServiceRequestHandler;
            private readonly RedisLeaderboardService _redisLeaderboardService;

            public DummyLeaderboardServiceRequestHandler(
                IOptions<ConfigurationOptions.PostgresSqlOptions> postgreSqlOptions,
                MatchResultService.MatchResultRequestHandler matchResultRequestHandler,
                RegisterService.RegisterRequestHandler registerServiceRequestHandler,
                RedisLeaderboardService redisLeaderboardService)
            {
                _postgreSqlOptions = postgreSqlOptions.Value;
                _matchResultServiceRequestHandler = matchResultRequestHandler;
                _registerServiceRequestHandler = registerServiceRequestHandler;
                _redisLeaderboardService = redisLeaderboardService;
            }

            public async Task<Result> HandleAsync(DummyLeaderboardServiceRequest request)
            {
                if (request.MinScore > request.MaxScore)
                    return Result.InvalidRequest();

                try
                {
                    var random = new Random();

                    const string testPass = "test";

                    using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

                    for (var i = 0; i < request.PlayerCount; ++i)
                    {
                        var score = random.Next(request.MinScore, request.MaxScore);

                        var username = Guid.NewGuid().ToString();

                        await _registerServiceRequestHandler.HandleAsync(new RegisterService.RegisterRequest
                        {
                            Username = username,
                            Password = testPass,
                            DeviceId = Guid.NewGuid().ToString(),
                        });

                        var sql = @"SELECT * FROM ""Players"" WHERE ""Username"" = @Username";

                        var player = await connection.QueryFirstOrDefaultAsync<Player>(sql, new { Username = username });

                        await _matchResultServiceRequestHandler.HandleAsync(new MatchResultService.MatchResultRequest
                        { Score = score }, player!.Id, refreshRedis: false);
                    }

                    await _redisLeaderboardService.RefreshTopRanks();

                    return Result.Success();
                }
                catch (Exception e)
                {
                    Log.Fatal("{message} {stackTrace}", e.Message, e.StackTrace);

                    return Result.Error();
                }
            }
        }

    }
}
