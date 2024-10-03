using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Data.Postgres.Models;

[Table("Players")]
public class Player
{
    public int Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string DeviceId { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;
    public byte[] PasswordHash { get; set; } = default!;
}
