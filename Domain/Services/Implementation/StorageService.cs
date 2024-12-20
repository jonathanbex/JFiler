using JFiler.Domain.Models;
using JFiler.Domain.Services;

public class StorageService : IStorageService
{
  private readonly List<DriveInfoModel> _drives;
  private const int _bufferSize = 81920;
  public StorageService(IConfiguration configuration)
  {
    // Load drives from configuration
    var driveConfigs = configuration.GetSection("StorageSettings:Drives").Get<List<DriveConfig>>();
    if (driveConfigs == null || driveConfigs.Count == 0) throw new InvalidDataException("Missing drives lol");
    _drives = driveConfigs.Select(config => GetDriveInfo(config.DrivePath)).ToList();
  }

  public async Task<IEnumerable<FileModel>> GetFilesAsync(string userDirectory)
  {
    var files = new List<FileModel>();

    foreach (var drive in _drives)
    {
      var directoryPath = Path.Combine(drive.DrivePath, userDirectory);

      if (!Directory.Exists(directoryPath))
        continue;

      var driveFiles = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly)
          .Select(file => new FileInfo(file))
          .Select(file => new FileModel
          {
            FileName = file.Name,
            FilePath = file.FullName,
            Size = file.Length,
            CreatedAt = file.CreationTime,
            ModifiedAt = file.LastWriteTime
          });

      files.AddRange(driveFiles);
    }

    return files;
  }

  public async Task UploadFileAsync(string userDirectory, IFormFile file, IProgress<double>? progress = null)
  {
    var fileSize = file.Length;

    var targetDrive = GetNextAvailableDrive(fileSize);
    var targetDirectory = Path.Combine(targetDrive, userDirectory);

    Directory.CreateDirectory(targetDirectory);

    var targetPath = Path.Combine(targetDirectory, file.FileName);
    await using (var inputStream = file.OpenReadStream())
    await using (var outputStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, _bufferSize, true))
    {
      var buffer = new byte[_bufferSize];
      int bytesRead;
      long bytesWritten = 0;

      while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
      {
        // Write the chunk to the output file
        await outputStream.WriteAsync(buffer, 0, bytesRead);

        // Update progress
        bytesWritten += bytesRead;
        progress?.Report((double)bytesWritten / fileSize * 100);
      }
    }
    // Update used space
    var drive = _drives.First(d => d.DrivePath == targetDrive);
    drive.FreeSpace += fileSize;
  }

  public async Task DeleteFileAsync(string userDirectory, string fileName)
  {
    foreach (var drive in _drives)
    {
      var filePath = Path.Combine(drive.DrivePath, userDirectory, fileName);

      if (File.Exists(filePath))
      {
        File.Delete(filePath);

        // Update used space
        var fileInfo = new FileInfo(filePath);
        drive.FreeSpace -= fileInfo.Length;

        return;
      }
    }

    throw new FileNotFoundException($"File '{fileName}' not found in any drive.");
  }

  public async Task<FileStream> GetFileStreamAsync(string userDirectory, string fileName)
  {
    foreach (var drive in _drives)
    {
      var filePath = Path.Combine(drive.DrivePath, userDirectory, fileName);

      if (File.Exists(filePath))
        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    throw new FileNotFoundException($"File '{fileName}' not found in any drive.");
  }

  private string GetNextAvailableDrive(long fileSize)
  {
    var availableDrive = _drives
        .Where(d => d.FreeSpace > fileSize)
        .OrderByDescending(d => d.FreeSpace)
        .FirstOrDefault();

    if (availableDrive == null)
      throw new InvalidOperationException("No drives with sufficient space available.");

    return availableDrive.DrivePath;
  }

  private DriveInfoModel GetDriveInfo(string drivePath)
  {
    var drive = new DriveInfo(drivePath); // Get the root of the drive
    return new DriveInfoModel
    {
      DrivePath = drivePath,
      TotalSpace = drive.TotalSize,
      FreeSpace = drive.TotalFreeSpace
    };
  }
}
