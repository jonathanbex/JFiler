using JFiler.Domain.Models.Request;
using JFiler.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Reflection.Metadata;

namespace JFiler.Controllers
{
  [Authorize]
  public class FileController : BaseController
  {
    private readonly ILogger<FileController> _logger;
    private IStorageService _storageService;
    private IUserService _userService;
    private IGlobalLinkService _globalLinkService;
    public FileController(ILogger<FileController> logger, IStorageService storageService, IUserService userService, IGlobalLinkService globalLinkService) : base()
    {
      _logger = logger;
      _storageService = storageService;
      _userService = userService;
      _globalLinkService = globalLinkService;
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

    [HttpGet]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
      var userId = _userService.GetCurrentUserId();
      if (userId == null) return BadRequest();
      using (var file = await _storageService.GetFileStreamAsync(userId, fileName))
      {
        if (file == null || file.Length == 0) return NotFound();


        string contentType = "application/octet-stream";
        if (!string.IsNullOrEmpty(fileName))
        {
          var provider = new FileExtensionContentTypeProvider();
          if (provider.TryGetContentType(fileName, out var detectedContentType))
          {
            contentType = detectedContentType;
          }
        }
        return File(file, contentType, fileName);
      }
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

    [HttpGet]
    public async Task<IActionResult> GenerateSingleUseLink(string fileName)
    {
      var userId = _userService.GetCurrentUserId();
      if (userId == null) return BadRequest();
      var link = await _globalLinkService.GenerateGlobalLinkAsync(userId, fileName);
      return Ok(new { link });
    }

    //allow anonymous as this is a global Link
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetFileFromSingleuseLink(string id)
    {
      var file = await _globalLinkService.GetFileFromLink(id);
      if (file.FileBytes.Length == 0) return NotFound();


      string contentType = "application/octet-stream";
      if (!string.IsNullOrEmpty(file.FileName))
      {
        var provider = new FileExtensionContentTypeProvider();
        if (provider.TryGetContentType(file.FileName, out var detectedContentType))
        {
          contentType = detectedContentType;
        }
      }
      return File(file.FileBytes, contentType, file.FileName);
    }


  }
}
