namespace JFiler.Domain.Models.ViewModel
{
  public class UserViewModel
  {
    public string Id { get; set; }
    public string Username { get; set; }
    public string? Password { get; set; }
    public string Email { get; set; }
    public bool Admin { get; set; }
  }
}
