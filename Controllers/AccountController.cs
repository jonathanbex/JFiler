using JFiler.Domain.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;
using JFiler.Domain.Models.ViewModel;

namespace JFiler.Controllers
{
  public class AuthenticationController : BaseController
  {
    IUserService _userService;
    public AuthenticationController(IUserService userService) : base()
    {
      _userService = userService;
    }
    [HttpGet]
    public async Task<IActionResult> Login()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginViewModel login)
    {
      var userName = login.Username;
      var password = login.Password;
      var loggedInUser = await _userService.Login(userName, password);
      if (loggedInUser == null) return RedirectToAction("Login");
      var claims = new List<Claim>
      {
          new Claim(ClaimTypes.Name, loggedInUser.Username),
          new Claim(ClaimTypes.GivenName, loggedInUser.Username),
          new Claim("UserId", loggedInUser.Id)
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
  }
}
