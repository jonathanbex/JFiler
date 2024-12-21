using JFiler.Domain.Helpers;
using JFiler.Domain.Repository;
using JFiler.Domain.Repository.Implementation;
using JFiler.Domain.Services;
using JFiler.Domain.Services.Implementation;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IGlobalLinkRepository, GlobalLinkRepository>();
builder.Services.AddSingleton<IStorageService, StorageService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IGlobalLinkService, GlobalLinkService>();

builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromMinutes(20);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});
builder.Services.ConfigureApplicationCookie(opts =>
{
  opts.LoginPath = "/Account/Login";

});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
      options.LoginPath = new PathString("/Account/Login");
      options.AccessDeniedPath = new PathString("/Account/Login");
      options.Cookie.Name = "JFiler_au";
      options.SlidingExpiration = true;
      options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Create an admin user if not exists
using (var scope = app.Services.CreateScope())
{
  var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
  var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
  var admin = await userService.GetAdmin();
  if (admin == null)
  {
    var adminUsername = configuration.GetValue<string>("AdminUsername");
    if (string.IsNullOrEmpty(adminUsername)) adminUsername = $"admin_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5)}";
    var adminPassword = configuration.GetValue<string>("AdminPassword");
    if (string.IsNullOrEmpty(adminPassword)) adminPassword = Guid.NewGuid().ToString();
    // Create the admin user
    var adminUser = await userService.CreateUser(adminUsername, adminUsername, adminPassword, true);

    JsonConfigurationHelper.UpdateAppSettings("AdminUsername", adminUsername);
    JsonConfigurationHelper.UpdateAppSettings("AdminPassword", adminPassword);

  }
}

app.Run();
