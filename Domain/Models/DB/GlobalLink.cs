using SQLite;

namespace JFiler.Domain.Models.DB
{
  public class GlobalLink
  {
    [PrimaryKey]
    public string Id { get; set; }
    public string? UserId { get; set; } 
    public string FileName { get; set; } 
    public DateTime? ExpirationTime { get; set; } 
    public bool IsUsed { get; set; } 
    public DateTime CreatedAt { get; set; } 
  }
}
