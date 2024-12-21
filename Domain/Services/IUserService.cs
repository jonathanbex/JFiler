using JFiler.Domain.Models.DB;
using JFiler.Domain.Models.ViewModel;

namespace JFiler.Domain.Services
{
  public interface IUserService
  {
    public Task<User?> Login(string username, string password);
    public Task<User> Logout(User user);
    public Task Delete(User user);
    public Task<User?> CreateUser(string userName, string email, string password, bool admin = false);
    public Task<User?> UpdateUser(User user);
    public string? GetCurrentUserId();
    public Task<User?> GetCurrentUser();
    public Task<List<UserViewModel>> GetUsers();
    public Task<User?> GetUserById(string id);
    public Task<User?> GetAdmin();
  }
}
