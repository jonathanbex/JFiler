using System.Diagnostics;
using JFiler.Domain.Mapper;
using JFiler.Domain.Models.ViewModel;
using JFiler.Domain.Services;
using JFiler.Domain.Utilities;
using JFiler.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JFiler.Controllers
{
  [Authorize]
  public class AdminController : BaseController
  {
    private readonly ILogger<AdminController> _logger;
    private IUserService _userService;
    private IStorageService _storageService;
    public AdminController(ILogger<AdminController> logger, IUserService userService, IStorageService storageService) : base()
    {
      _logger = logger;
      _userService = userService;
      _storageService = storageService;
    }

    public async Task<IActionResult> Index()
    {
      if (!await IsAdmin()) return BadRequest();
      var users = await _userService.GetUsers();
      return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> RegisterNewUser()
    {
      if (!await IsAdmin()) return BadRequest();
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterNewUser([FromForm] CreateNewUserViewModel usr)
    {
      if (!await IsAdmin()) return BadRequest();
      await _userService.CreateUser(usr.Username, usr.Email, usr.Password, usr.Admin);

      return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
      if (!await IsAdmin()) return BadRequest();
      var user = await _userService.GetUserById(id);
      if (user == null) return BadRequest();
      var model = ViewModelMapper.MapEntityToViewModel(user);
      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUser([FromForm] UserViewModel model)
    {
      if (!await IsAdmin()) return BadRequest();
      var user = await _userService.GetUserById(model.Id);
      if (user == null) return BadRequest();
      if (!string.IsNullOrEmpty(model.Password))
      {
        var salt = CryptoUtility.GetSalt();
        var hashedPassword = CryptoUtility.ComputeSHA256Hash(model.Password + salt);
        user.PasswordHash = hashedPassword;
        user.Salt = salt;
      }
      user.Email = model.Email;
      user.Admin = model.Admin;
      await _userService.UpdateUser(user);

      return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
      if (!await IsAdmin()) return BadRequest();
      var user = await _userService.GetUserById(id);
      if (user == null) return NotFound();
      await _userService.Delete(user);

      return RedirectToAction("Index");
    }
    private async Task<bool> IsAdmin()
    {
      var user = await _userService.GetCurrentUser();
      if (user == null || !user.Admin.GetValueOrDefault(false)) return false;
      return true;
    }


    [HttpGet]
    public IActionResult Drives()
    {
      var drives = _storageService.GetDrives();
      return View(drives);
    }

    [HttpPost]
    public IActionResult AddDrive([FromForm] AddDriveRequest request)
    {
      if (string.IsNullOrWhiteSpace(request.DrivePath))
        return BadRequest("DrivePath is required.");

      try
      {
        _storageService.RegisterNewDrive(request.DrivePath);
        return RedirectToAction("Drives");
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpPost]
    public IActionResult RemoveDrive(string drivePath)
    {
      try
      {
        _storageService.RemoveDrive(drivePath);
        return RedirectToAction("Drives");
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}
