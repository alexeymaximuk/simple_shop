using Microsoft.AspNetCore.Authentication.Cookies;
using Shop.Frontend.Constants;
using Shop.Frontend.Handlers;
using Shop.Frontend.Settings;
using Shop.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

var apiSettings = builder.Configuration.GetRequiredSettings<ApiSettings>(ApiSettings.ApiSettingsName);

builder.Services.AddControllersWithViews();
builder.Services.AddCors();

builder.Services
    .AddHttpClient(ApiClients.Users, client => { client.BaseAddress = new Uri(apiSettings.UsersAPI); })
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services
    .AddHttpClient(ApiClients.Products, client => { client.BaseAddress = new Uri(apiSettings.ProductsAPI); })
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddTransient<UnauthorizedHandler>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();
app.UseSession();

if (app.Environment.IsDevelopment())
{
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
