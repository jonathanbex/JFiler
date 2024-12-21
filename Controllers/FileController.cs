using JFiler.Domain.Models.Request;
using JFiler.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JFiler.Controllers
{
  [Authorize]
  public class FileController : BaseController
  {
    private readonly ILogger<FileController> _logger;
    private IStorageService _storageService;
    private IUserService _userService;
    public FileController(ILogger<FileController> logger, IStorageService storageService, IUserService userService) : base()
    {
      _logger = logger;
      _storageService = storageService;
      _userService = userService;
    }

    public IActionResult Index()
    {
      return View();
    }

    public async Task<IActionResult> UploadFile(IFormFile file, IProgress<double>? progress = null)
    {
      var userId = _userService.GetCurrentUserId();
      if (userId == null) return BadRequest();
      await _storageService.UploadFileAsync(userId, file, progress);

      return Ok(new { message = "File uploaded successfully." });
    }

    [HttpPost]
    public async Task<IActionResult> GetFiles([FromBody] QueryRequest query)
    {
      var userId = _userService.GetCurrentUserId();
      if (userId == null) return BadRequest();
      var fileResults = await _storageService.GetFilesAsync(userId, query.SearchTerm, query.Page, query.PageSize);
      foreach (var file in fileResults.Files) file.FilePath = "";

      return Ok(fileResults);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFile(string id)
    {
      var userId = _userService.GetCurrentUserId();
      if (userId == null) return BadRequest();
      await _storageService.DeleteFileAsync(userId, id);
      return Ok();
    }


  }
}
