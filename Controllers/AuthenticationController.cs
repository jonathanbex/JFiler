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
        // Refreshing the authentication session should be allowed.

        //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
        // The time at which the authentication ticket expires. A 
        // value set here overrides the ExpireTimeSpan option of 
        // CookieAuthenticationOptions set with AddCookie.

        IsPersistent = true,
        // Whether the authentication session is persisted across 
        // multiple requests. When used with cookies, controls
        // whether the cookie's lifetime is absolute (matching the
        // lifetime of the authentication ticket) or session-based.

        IssuedUtc = DateTime.Now,
        // The time at which the authentication ticket was issued.

        //RedirectUri = <string>
        // The full path or absolute URI to be used as an http 
        // redirect response value.
      };

      await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
      return RedirectToAction("Index", "Home");
    }
  }
}
