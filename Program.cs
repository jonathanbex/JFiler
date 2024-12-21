using JFiler.Domain.Repository;
using JFiler.Domain.Repository.Implementation;
using JFiler.Domain.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IStorageService, StorageService>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IGlobalLinkRepository, GlobalLinkRepository>();

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

app.Run();
