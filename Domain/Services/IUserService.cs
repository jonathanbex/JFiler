using JFiler.Domain.Models.DB;

namespace JFiler.Domain.Services
{
  public interface IUserService
  {
    public Task<User?> Login(string username, string password);
    public Task<User> Logout(User user);
    public Task Delete(User user);
    public Task<User?> CreateUser(string userName,string email, string password, bool admin = false);
  }
}
