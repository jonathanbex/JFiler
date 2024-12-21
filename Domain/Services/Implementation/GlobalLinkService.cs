using JFiler.Domain.Models.DB;
using JFiler.Domain.Repository;
using System.IO;

namespace JFiler.Domain.Services.Implementation
{
  public class GlobalLinkService : IGlobalLinkService
  {
    IGlobalLinkRepository _globalLinkRepository;
    IStorageService _storageService;
    public GlobalLinkService(IGlobalLinkRepository globalLinkRepository, IStorageService storageService)
    {
      _globalLinkRepository = globalLinkRepository;
      _storageService = storageService;
    }

    public async Task<string> GenerateGlobalLinkAsync(User user, string fileName, TimeSpan? expiration = null)
    {
      var globalLink = new GlobalLink
      {
        Id = Guid.NewGuid().ToString(),
        UserId = user.Id,
        FileName = fileName,
        ExpirationTime = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : DateTime.UtcNow.AddHours(12),
        IsUsed = false,
        CreatedAt = DateTime.UtcNow
      };

      return await _globalLinkRepository.GenerateGlobalLinkAsync(globalLink);
    }

    public async Task<byte[]> GetFileFromLink(string id)
    {
      var link = await _globalLinkRepository.GetGlobalLinkAsync(id);
      if (link == null) return Array.Empty<byte>();
      using (var fileStream = await _storageService.GetFileStreamAsync(link.UserId, link.FileName))
      {
        if (fileStream == null) return Array.Empty<byte>();
        if (fileStream.Length == 0) return Array.Empty<byte>();
        fileStream.Position = 0;
        byte[] fileBytes = new byte[fileStream.Length];

        // Read the file contents into the byte array
        fileStream.Read(fileBytes, 0, (int)fileStream.Length);

        await _globalLinkRepository.MarkLinkAsUsedAsync(id);
        return fileBytes;
      }
    }
  }
}
