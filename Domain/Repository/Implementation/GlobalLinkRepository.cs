using JFiler.Domain.Models.DB;
using SQLite;

namespace JFiler.Domain.Repository.Implementation
{
  public class GlobalLinkRepository : IGlobalLinkRepository
  {
    private SQLiteAsyncConnection _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseConnectionAsync()
    {
      if (_database == null)
      {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "storage.db");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<GlobalLink>();
      }
      return _database;
    }

    public async Task<string> GenerateGlobalLinkAsync(GlobalLink globalLink)
    {
      var db = await GetDatabaseConnectionAsync();

      await db.InsertAsync(globalLink);

      return globalLink.Id; // Return link ID for external use
    }

    public async Task<GlobalLink?> GetGlobalLinkAsync(string id)
    {
      var db = await GetDatabaseConnectionAsync();
      return await db.Table<GlobalLink>().FirstOrDefaultAsync(x => x.Id == id && x.IsUsed == false && x.ExpirationTime < DateTime.UtcNow);
    }

    public async Task<bool> MarkLinkAsUsedAsync(string id)
    {
      var db = await GetDatabaseConnectionAsync();
      var link = await db.Table<GlobalLink>().FirstOrDefaultAsync(x => x.Id == id);

      if (link == null || link.IsUsed || (link.ExpirationTime.HasValue && link.ExpirationTime < DateTime.UtcNow))
      {
        return false; // Invalid or expired link
      }

      link.IsUsed = true;
      await db.UpdateAsync(link);
      return true;
    }
  }



}
