namespace JFiler.Domain.Models.Request
{
  public class QueryRequest
  {
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 30;
  }
}
