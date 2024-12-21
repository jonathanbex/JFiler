using JFiler.Domain.Models.DB;
using SQLite;
namespace JFiler.Domain.Repository.Implementation
{


  public class UserRepository : IUserRepository
  {
    private SQLiteAsyncConnection _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseConnectionAsync()
    {
      if (_database == null)
      {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "storage.db");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<User>();
      }
      return _database;
    }

    public async Task<User> AddUserAsync(User user)
    {
      var db = await GetDatabaseConnectionAsync();
      var existingUser = await db.Table<User>().FirstOrDefaultAsync(x => x.Username == user.Username);

      if (existingUser != null)
      {
        return existingUser; // User already exists
      }

      await db.InsertAsync(user);
      return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
      var db = await GetDatabaseConnectionAsync();
      return await db.Table<User>().FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
      var db = await GetDatabaseConnectionAsync();
      var result = await db.Table<User>().DeleteAsync(x => x.Id == userId);
      return result > 0;
    }

    public async Task SetFailedAttempt(User user)
    {
      user.FailedAttempts = user.FailedAttempts.GetValueOrDefault(0) + 1;
      user.LastFailedAttempt = DateTime.UtcNow;
      var db = await GetDatabaseConnectionAsync();
      await db.UpdateAsync(user);
    }
  }

}
