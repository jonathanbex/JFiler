using JFiler.Domain.Helpers;
using JFiler.Domain.Models;
using JFiler.Domain.Services;
using Microsoft.AspNetCore.Mvc;

public class StorageService : IStorageService
{
  private readonly List<DriveInfoModel> _drives;
  private const int _bufferSize = 81920;
  IConfiguration _configuration;
  public StorageService(IConfiguration configuration)
  {
    _configuration = configuration;
    // Load drives from configuration
    var driveConfigs = configuration.GetSection("StorageSettings:Drives").Get<List<DriveConfig>>();
    if (driveConfigs == null || driveConfigs.Count == 0) throw new InvalidDataException("Missing drives lol");
    _drives = driveConfigs.Select(config => ValidateAndGetDriveInfo(config.DrivePath)).Where(info => info != null).ToList();
  }
  public async Task<FileResultModel> GetFilesAsync(string userDirectory, string? searchTerm, int page = 0, int pageSize = 30)
  {
    if (page > 0) page = page - 1;
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


    if (!string.IsNullOrEmpty(searchTerm))
    {
      files = files.Where(file => file.FileName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }
    var count = files.Count();

    files = files
        .Skip(page * pageSize)
        .Take(pageSize)
        .ToList();


    return new FileResultModel { Files = files, TotalCount = count };
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


        // Update used space
        var fileInfo = new FileInfo(filePath);
        drive.FreeSpace -= fileInfo.Length;
        File.Delete(filePath);
        return;
      }
    }

    throw new FileNotFoundException($"File '{fileName}' not found in any drive.");
  }

  public async Task<FileStream> GetFileStreamAsync(string userDirectory, string fileName)
  {
    if (string.IsNullOrEmpty(userDirectory)) throw new FileNotFoundException($"File '{fileName}' not found");
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

  private DriveInfoModel ValidateAndGetDriveInfo(string drivePath)
  {
    var drive = new DriveInfo(drivePath);
    if (!drive.IsReady)
    {
      //drive does not exist or isnt ready
      return null;
    }

    return new DriveInfoModel
    {
      DrivePath = drivePath,
      TotalSpace = drive.TotalSize,
      FreeSpace = drive.TotalFreeSpace
    };
  }

  public void RegisterNewDrive(string drivePath)
  {
    if (_drives.Any(d => d.DrivePath.Equals(drivePath, StringComparison.OrdinalIgnoreCase)))
      throw new InvalidOperationException($"Drive {drivePath} is already registered.");

    var driveInfo = ValidateAndGetDriveInfo(drivePath);
    if (driveInfo == null)
      throw new InvalidDataException($"Drive {drivePath} is invalid or inaccessible.");

    _drives.Add(driveInfo);
    SaveDrives();
  }

  public void RemoveDrive(string drivePath)
  {
    var entry = _drives.FirstOrDefault(d => d.DrivePath.Equals(drivePath, StringComparison.OrdinalIgnoreCase));
    if (entry == null)
      throw new InvalidOperationException($"Drive {drivePath} isn't registered.");
    _drives.Remove(entry);
    SaveDrives();

  }
  private void SaveDrives()
  {
    // Transform _drives into a format suitable for appsettings.json
    var drivesAsObjects = _drives
        .Select(drive => new Dictionary<string, object>
        {
            { "DrivePath", drive.DrivePath }
        })
        .ToList();

    JsonConfigurationHelper.UpdateAppSettings("StorageSettings:Drives", drivesAsObjects);
  }

  public IReadOnlyList<DriveInfoModel> GetDrives() => _drives.AsReadOnly();


}
