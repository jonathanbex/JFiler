using SQLite;

namespace JFiler.Domain.Models.DB
{
  public class User
  {
    [PrimaryKey]
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? Salt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool? Admin { get; set; }

    public int? FailedAttempts { get; set; }
    public DateTime? LastFailedAttempt { get; set; }
  }
}
