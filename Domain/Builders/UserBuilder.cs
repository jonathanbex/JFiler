using JFiler.Domain.Models.DB;
using JFiler.Domain.Utilities;

namespace JFiler.Domain.Builders
{
  public class UserBuilder
  {
    private string? Username;
    private string? Email;
    private string? HashedPassword;
    private bool? Admin;
    private string? Salt;
    public UserBuilder SetUsername(string userName)
    {
      Username = userName;
      return this;
    }
    public UserBuilder SetEmail(string email)
    {
      Email = email;
      return this;
    }
    public UserBuilder SetPassword(string password)
    {
      var salt = CryptoUtility.GetSalt();
      var hashedPassword = CryptoUtility.ComputeSHA256Hash(password + salt);
      HashedPassword = hashedPassword;
      Salt = salt;
      return this;
    }
    public UserBuilder SetAdmin(bool admin)
    {
      Admin = admin;
      return this;
    }
    public User Build()
    {
      if (string.IsNullOrEmpty(Username)) throw new InvalidDataException("Missing Username");
      if (string.IsNullOrEmpty(Email)) throw new InvalidDataException("Missing Email");
      if (string.IsNullOrEmpty(HashedPassword) || string.IsNullOrEmpty(Salt)) throw new InvalidDataException("Missing Pasword");
      return new User { Id = Guid.NewGuid().ToString(), Username = Username, Email = Email, Salt = Salt, PasswordHash = HashedPassword, CreatedAt = DateTime.UtcNow, Admin = Admin };
    }
  }
}
