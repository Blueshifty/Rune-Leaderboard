using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Data.Postgres.Models;

[Table("LeaderboardScores")]
public class LeaderboardScore
{
    public int PlayerId { get; set; } = default!;
    public int Score { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = default!;
}
