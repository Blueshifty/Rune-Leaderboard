using Api.Constants;
using Api.Data.Postgres.Models;
using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using Serilog;
using StackExchange.Redis;

namespace Api.Data.Redis;

public class RedisLeaderboardService
{
    private readonly ConfigurationOptions.RedisOptions _redisOptions;
    private readonly IDatabase _database;
    private readonly ConfigurationOptions.PostgresSqlOptions _postgreSqlOptions;
    public RedisLeaderboardService(
        IOptions<ConfigurationOptions.RedisOptions> redisOptions,
        IOptions<ConfigurationOptions.PostgresSqlOptions> postgreSqlOptions
        )
    {
        _redisOptions = redisOptions.Value;
        var connectionMultiplexer = ConnectionMultiplexer.Connect(_redisOptions.Endpoint);
        _database = connectionMultiplexer.GetDatabase();
        _postgreSqlOptions = postgreSqlOptions.Value;
    }

    public async Task<(LeaderboardRanking? ranking, string? error)> GetRankingByPlayerId(int playerId)
    {
        try
        {
            var playerRank = await _database.SortedSetScoreAsync(RedisConstants.TopRankSetKey, playerId.ToString());

            if (!playerRank.HasValue)
            {
                return (null, null);
            }

            var playerDetailsKey = RedisConstants.GetPlayerDetailsKey((int)playerRank.Value);
            var playerJsonStr = await _database.StringGetAsync(playerDetailsKey);

            if (!playerJsonStr.HasValue)
            {
                return (null, "Player details not found");
            }

            try
            {
                var leaderboardRanking = JsonConvert.DeserializeObject<LeaderboardRanking>(playerJsonStr!);

                return (leaderboardRanking, null);
            }
            catch
            {
                Log.Error($"Cant desearilize redis value => {playerJsonStr}");

                return (null, "Failed to deserialize player data");
            }
        }
        catch (Exception ex)
        {
            var error = $"{ex.Message} : {ex.StackTrace}";

            Log.Fatal(error);

            return new(null, error);
        }
    }

    public async Task<(List<LeaderboardRanking>? rankings, string? error)> GetRanks(int from, int to)
    {
        if (from <= 0 || from > to)
            return (null, "Invalid range");

        try
        {
            var leaderboardRankings = new List<LeaderboardRanking>();

            for (var i = from; i <= to; ++i)
            {
                var playerRankKey = RedisConstants.GetPlayerDetailsKey(i);
                var playerJsonStr = await _database.StringGetAsync(playerRankKey);

                if (playerJsonStr.HasValue)
                {
                    try
                    {
                        var leaderboardRanking = JsonConvert.DeserializeObject<LeaderboardRanking>(playerJsonStr!);
                        leaderboardRankings.Add(leaderboardRanking!);
                    }
                    catch
                    {
                        Log.Error($"Cant deserialize redis value => {playerJsonStr}");
                    }
                }
            }

            return (leaderboardRankings, null);
        }
        catch (Exception ex)
        {
            var error = $"{ex.Message} : {ex.StackTrace}";

            Log.Fatal(error);

            return new(null, error);
        }
    }

    public async Task<string?> RefreshTopRanks()
    {
        try
        {
            var lockAcquired = await _database.StringSetAsync(RedisConstants.TopRankLockKey, "locked", TimeSpan.FromMinutes(5), When.NotExists);

            if (!lockAcquired)
            {
                Log.Information("Cant acquired key for redis update");

                return null;
            }

            try
            {
                var sql = @"
                                     SELECT * 
                                     FROM ""LeaderboardRankingsView"" AS ""l""
                                     JOIN ""Players"" AS ""p""
                                     ON ""l"".""PlayerId"" = ""p"".""Id""
                                     JOIN ""LeaderboardScores"" AS ""s""
                                     ON ""l"".""PlayerId"" = ""s"".""PlayerId""
                                     WHERE ""l"".""Rank"" <= @Rank";

                using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

                var topRanks = await connection.QueryAsync<LeaderboardRanking>(sql, new { Rank = RedisConstants.TopRankCount });

                foreach (var topRank in topRanks)
                {
                    await _database.SortedSetAddAsync(RedisConstants.TopRankSetKey, topRank.PlayerId, topRank.Rank);
                }

                var playerCount = await _database.SortedSetLengthAsync(RedisConstants.TopRankSetKey);

                if (playerCount > RedisConstants.TopRankCount)
                {
                    await _database.SortedSetRemoveRangeByRankAsync(RedisConstants.TopRankSetKey, 0, playerCount - (RedisConstants.TopRankCount + 1));
                }

                foreach (var rank in topRanks)
                {
                    var playerRankKey = RedisConstants.GetPlayerDetailsKey(rank.Rank);

                    var playerData = new LeaderboardRanking
                    {
                        PlayerId = rank.PlayerId,
                        Username = rank.Username,
                        Score = rank.Score,
                        CreatedAt = rank.CreatedAt,
                        Rank = rank.Rank
                    };

                    await _database.StringSetAsync(playerRankKey, JsonConvert.SerializeObject(playerData));
                }
            }
            finally
            {
                await _database.KeyDeleteAsync(RedisConstants.TopRankLockKey);
            }

            return null;
        }
        catch (Exception ex)
        {
            var error = $"{ex.Message} : {ex.StackTrace}";

            Log.Fatal(error);

            return error;
        }
    }
}
