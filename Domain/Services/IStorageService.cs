using JFiler.Domain.Models;
using System.IO;

namespace JFiler.Domain.Services
{
  public interface IStorageService
  {
    Task<IEnumerable<FileModel>> GetFilesAsync(string userDirectory);
    Task UploadFileAsync(string userDirectory, IFormFile file, IProgress<double>? progress = null);
    Task DeleteFileAsync(string userDirectory, string fileName);
    Task<FileStream> GetFileStreamAsync(string userDirectory, string fileName);
  }
}
