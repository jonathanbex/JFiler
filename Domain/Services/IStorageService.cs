using JFiler.Domain.Models.ViewModel;

namespace JFiler.Domain.Services
{
  public interface IStorageService
  {
    Task<FileResultViewModel> GetFilesAsync(string userDirectory, string? searchTerm, int page, int pageSize);
    Task UploadFileAsync(string userDirectory, IFormFile file, IProgress<double>? progress = null);
    Task DeleteFileAsync(string userDirectory, string fileName);
    Task<FileStream> GetFileStreamAsync(string? userDirectory, string fileName);
  }
}
