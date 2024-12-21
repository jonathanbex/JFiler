using JFiler.Domain.Models;

namespace JFiler.Domain.Services
{
  public interface IStorageService
  {
    public Task<FileResultModel> GetFilesAsync(string userDirectory, string? searchTerm, int page, int pageSize);
    public Task UploadFileAsync(string userDirectory, IFormFile file, IProgress<double>? progress = null);
    public Task DeleteFileAsync(string userDirectory, string fileName);
    public Task<FileStream> GetFileStreamAsync(string? userDirectory, string fileName);
    public void RegisterNewDrive(string drivePath);
    public void RemoveDrive(string drivePath);
    public IReadOnlyList<DriveInfoModel> GetDrives();
  }
}
