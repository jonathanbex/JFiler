using System.ComponentModel.DataAnnotations;

namespace JFiler.Domain.Models.ViewModel
{
  public class CreateNewUserViewModel
  {
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    public bool Admin { get; set; } = false;
  }
}
