using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Shop.Frontend.Constants;
using Shop.Frontend.Models;

namespace Shop.Frontend.Controllers.Auth;

public class LoginController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        var client = httpClientFactory.CreateClient(ApiClients.Users);
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("/api/users/login", content);
        
        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadAsStringAsync();
            
            HttpContext.Session.SetString("JwtToken", token);
            
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt =  handler.ReadJwtToken(token);

            var claims = jwt.Claims.ToList();
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            
            
            return RedirectToAction("Index", "Products");
        }
        
        ModelState.AddModelError("", "Invalid login attempt");
        
        return View(model);
    }
}