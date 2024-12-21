using JFiler.Domain.Models.DB;

namespace JFiler.Domain.Repository
{
  public interface IUserRepository
  {
    public Task<User> AddUser(User user);
    public Task<User> UpdateUser(User user);
    public Task<User?> GetUserById(string id);
    public Task<User?> GetUserByUsername(string username);
    public Task<bool> DeleteUser(string userId);
    public Task SetFailedAttempt(User user);
    public Task<List<User>> GetUsers();
    public Task<User?> GetAdmin();
  }
}
