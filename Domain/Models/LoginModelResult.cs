using JFiler.Domain.Models.DB;

namespace JFiler.Domain.Models
{
  public class LoginModelResult
  {
    public User? User { get; set; }
    public bool? Locked { get; set; }
    public bool? WrongPassword { get; set; }
  }
}
