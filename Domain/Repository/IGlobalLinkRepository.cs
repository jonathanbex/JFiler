using JFiler.Domain.Models.DB;

namespace JFiler.Domain.Repository
{
  public interface IGlobalLinkRepository
  {
    public Task<string> GenerateGlobalLinkAsync(GlobalLink globalLink);
    public Task<GlobalLink?> GetGlobalLinkAsync(string id);
    public Task<bool> MarkLinkAsUsedAsync(string id);
  }
}
