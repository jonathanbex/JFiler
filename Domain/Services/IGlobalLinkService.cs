using JFiler.Domain.Models;

namespace JFiler.Domain.Services
{
  public interface IGlobalLinkService
  {
    public Task<string> GenerateGlobalLinkAsync(string userId, string fileName, TimeSpan? expiration = null);
    public Task<GlobalLinkFileResult> GetFileFromLink(string id);
  }
}
