namespace JFiler.Domain.Models
{
  public class FileModel
  {
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
  }
}
