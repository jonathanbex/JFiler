using JFiler.Domain.Builders;
using JFiler.Domain.Models.DB;
using JFiler.Domain.Repository;
using JFiler.Domain.Utilities;

namespace JFiler.Domain.Services.Implementation
{
  public class UserService : IUserService
  {
    IUserRepository _userRepository;
    IHttpContextAccessor _contextAccessor;
    public UserService(IUserRepository userRepository, IHttpContextAccessor contextAccessor)
    {
      _userRepository = userRepository;
      _contextAccessor = contextAccessor;
    }

    public async Task<User?> Login(string username, string password)
    {
      var user = await _userRepository.GetUserByUsernameAsync(username);
      if (user == null) return null;

      //check if user is locked
      if (user.FailedAttempts.GetValueOrDefault(0) > 4
        && user.LastFailedAttempt != null && user.LastFailedAttempt.Value > DateTime.UtcNow.AddMinutes(-15))
      {
        return null;
      }
      //clear old locks
      else if (user.LastFailedAttempt != null && user.LastFailedAttempt.Value < DateTime.UtcNow.AddMinutes(-15))
      {
        user.LastFailedAttempt = null;
        user.FailedAttempts = null;
      }

      var hashedPass = CryptoUtility.ComputeSHA256Hash(password + user.Salt);
      if (user.PasswordHash != hashedPass)
      {
        await _userRepository.SetFailedAttempt(user);
      }

      return user;
    }
    public async Task<User> Logout(User user)
    {
      throw new NotImplementedException();
    }
    public async Task Delete(User user)
    {
      await _userRepository.DeleteUserAsync(user.Id);
    }

    public async Task<User?> CreateUser(string userName, string email, string password, bool admin = false)
    {
      var user = new UserBuilder()
        .SetUsername(userName)
        .SetEmail(email)
        .SetPassword(password)
        .SetAdmin(admin)
        .Build();
      return await _userRepository.AddUserAsync(user);
    }

    public string? GetCurrentUserId()
    {
      var user = _contextAccessor.HttpContext?.User;
      if (user?.Identity?.IsAuthenticated ?? false)
      {
        var userIdClaim = user.FindFirst("UserId");
        return userIdClaim?.Value;
      }
      return null;
    }
  }
}
