using System.ComponentModel.DataAnnotations;

namespace JFiler.Domain.Models.ViewModel
{
  public class LoginViewModel
  {
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
  }
}
