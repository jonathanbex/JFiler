namespace JFiler.Domain.Models
{
  public class StorageSettings
  {
    public List<DriveConfig> Drives { get; set; }
  }

  public class DriveConfig
  {
    public string DrivePath { get; set; }
  }

  public class DriveInfoModel
  {
    public string DrivePath { get; set; } 
    public long TotalSpace { get; set; } 
    public long FreeSpace { get; set; } 
  }
}
