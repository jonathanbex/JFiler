using JFiler.Domain.Models.DB;

namespace JFiler.Domain.Services
{
  public interface IGlobalLinkService
  {
    public Task<string> GenerateGlobalLinkAsync(User user, string filePath, TimeSpan? expiration = null);
    public Task<byte[]> GetFileFromLink(string id);
  }
}
