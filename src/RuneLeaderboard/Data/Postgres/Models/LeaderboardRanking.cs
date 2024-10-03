namespace Api.Data.Postgres.Models
{
    public class LeaderboardRanking
    {
        public int PlayerId { get; set; } = default!;
        public string Username { get; set; } = default!;
        public int Score { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = default!;
        public int Rank { get; set; } = default!;
    }
}
