namespace Api.Constants
{
    public static class RedisConstants
    {
        public const int TopRankCount = 500;
        public const string TopRankSetKey = "leaderboard:topranks";
        public const string TopRankListKey = "leaderboard:toprank_list";
        public const string TopRankLockKey = "toprank_lock_key";
        public const string PlayerHashKey = "Player";


        public static string GetPlayerDetailsKey(int rank) => $"rank:details:{rank}";
    }
}
