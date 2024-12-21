namespace JFiler.Domain.Models
{
  public class GlobalLinkFileResult
  {
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string? FileName { get; set; }
  }
}
