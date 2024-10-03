using Api.Data.Redis;
using Serilog;

namespace Api.Hangfire.Jobs;

public class UpdateTopRanksOnRedisJob
{
    private readonly RedisLeaderboardService _redisLeaderboardService;


    public UpdateTopRanksOnRedisJob(RedisLeaderboardService redisLeaderboardService)
    {
        _redisLeaderboardService = redisLeaderboardService;
    }

    public async Task RunAsync()
    {
        try
        {
            await _redisLeaderboardService.RefreshTopRanks();
        }
        catch (Exception ex)
        {
            Log.Fatal("{message} : {stackTrace}", ex.Message, ex.StackTrace);
        }
    }
}
