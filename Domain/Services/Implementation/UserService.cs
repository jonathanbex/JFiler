using JFiler.Domain.Builders;
using JFiler.Domain.Mapper;
using JFiler.Domain.Models.DB;
using JFiler.Domain.Models.ViewModel;
using JFiler.Domain.Repository;
using JFiler.Domain.Repository.Implementation;
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
      var user = await _userRepository.GetUserByUsername(username);
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
      await _userRepository.DeleteUser(user.Id);
    }

    public async Task<User?> CreateUser(string userName, string email, string password, bool admin = false)
    {
      var user = new UserBuilder()
        .SetUsername(userName)
        .SetEmail(email)
        .SetPassword(password)
        .SetAdmin(admin)
        .Build();
      return await _userRepository.AddUser(user);
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

    public async Task<User?> GetCurrentUser()
    {
      var userId = GetCurrentUserId();
      if (userId == null) return null;
      var user = await _userRepository.GetUserById(userId);
      return user;
    }

    public async Task<List<UserViewModel>> GetUsers()
    {
      var users = await _userRepository.GetUsers();
      return users.Select(x => ViewModelMapper.MapEntityToViewModel(x)).ToList();
    }

    public async Task<User?> GetUserById(string id)
    {
      return await _userRepository.GetUserById(id);
    }

    public async Task<User?> UpdateUser(User user)
    {
      return await _userRepository.UpdateUser(user);
    }

    public async Task<User?> GetAdmin()
    {
      return await _userRepository.GetAdmin();
    }
  }
}
