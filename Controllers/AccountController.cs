using JFiler.Domain.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;
using JFiler.Domain.Models.ViewModel;

namespace JFiler.Controllers
{
  public class AccountController : BaseController
  {
    IUserService _userService;
    public AccountController(IUserService userService) : base()
    {
      _userService = userService;
    }
    [HttpGet]
    public async Task<IActionResult> Login()
    {
      return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginViewModel login)
    {
      var userName = login.Username;
      var password = login.Password;
      var loginResult = await _userService.Login(userName, password);

      var user = loginResult.User;
      if (user == null)
      {
        if (loginResult.Locked == true)
        {
          login.Message = "Your account is locked. Wait 5 minutes and try again";
        }
        else if (loginResult.WrongPassword == true)
        {
          login.Message = "Incorrect password. Please try again.";
        }
        else
        {
          login.Message = "Login failed. Please check your credentials.";
        }

        // Return the view with the updated model
        return View(login);
      }
      var claims = new List<Claim>
      {
          new Claim(ClaimTypes.Name, user.Username),
          new Claim(ClaimTypes.GivenName, user.Username),
          new Claim("UserId", user.Id)
      };
      var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

      var authProperties = new AuthenticationProperties
      {
        AllowRefresh = true,

        IsPersistent = true,

        IssuedUtc = DateTime.Now,

      };

      await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
      return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> LogOut()
    {
      await HttpContext.SignOutAsync();
      return RedirectToAction("Login", "Account");
    }
  }
}
