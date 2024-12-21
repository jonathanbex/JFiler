using JFiler.Domain.Models.DB;

namespace JFiler.Domain.Repository
{
  public interface IUserRepository
  {
    public Task<User> AddUserAsync(User user);

    public Task<User?> GetUserByUsernameAsync(string username);

    public Task<bool> DeleteUserAsync(string userId);
    public Task SetFailedAttempt(User user);
  }
}
