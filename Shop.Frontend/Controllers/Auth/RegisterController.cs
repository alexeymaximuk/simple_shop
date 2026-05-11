using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shop.Frontend.Constants;
using Shop.Frontend.Models;
using Shop.Shared.Controllers;

namespace Shop.Frontend.Controllers.Auth;

public class RegisterController(IHttpClientFactory httpClientFactory) : BaseController
{
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
            
        var client = httpClientFactory.CreateClient(ApiClients.Users);
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
            
        var response = await client.PostAsync("/api/users/register", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "success";
                
            return RedirectToAction("Login", "Login");
        }
            
        await AddApiErrors(response);
            
        return View(model);
    }
}