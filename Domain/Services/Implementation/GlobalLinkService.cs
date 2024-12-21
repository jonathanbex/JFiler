using JFiler.Domain.Models;
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

    public async Task<string> GenerateGlobalLinkAsync(string userId, string fileName, TimeSpan? expiration = null)
    {
      var globalLink = new GlobalLink
      {
        Id = Guid.NewGuid().ToString(),
        UserId = userId,
        FileName = fileName,
        ExpirationTime = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : DateTime.UtcNow.AddHours(12),
        IsUsed = false,
        CreatedAt = DateTime.UtcNow
      };

      return await _globalLinkRepository.GenerateGlobalLinkAsync(globalLink);
    }

    public async Task<GlobalLinkFileResult> GetFileFromLink(string id)
    {
      var returnModel = new GlobalLinkFileResult();
      var link = await _globalLinkRepository.GetGlobalLinkAsync(id);
      if (link == null) return returnModel;
      using (var fileStream = await _storageService.GetFileStreamAsync(link.UserId, link.FileName))
      {
        if (fileStream == null) return returnModel;
        if (fileStream.Length == 0) return returnModel;
        fileStream.Position = 0;
        byte[] fileBytes = new byte[fileStream.Length];

        // Read the file contents into the byte array
        fileStream.Read(fileBytes, 0, (int)fileStream.Length);

        await _globalLinkRepository.MarkLinkAsUsedAsync(id);
        returnModel.FileName = link.FileName;
        returnModel.FileBytes = fileBytes;
        return returnModel;
      }
    }
  }
}
