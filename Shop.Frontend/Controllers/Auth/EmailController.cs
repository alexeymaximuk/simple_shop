using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shop.Frontend.Constants;
using Shop.Frontend.Models;

namespace Shop.Frontend.Controllers.Auth;

public class EmailController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var client = httpClientFactory.CreateClient(ApiClients.Users);
        var encodedToken = Uri.EscapeDataString(token);
        var response = await client.GetAsync($"/api/users/confirm-email?token={encodedToken}");
        
        return View(response.IsSuccessStatusCode ? "ConfirmEmailSuccess" : "ConfirmEmailError");
    }
    
    [HttpGet]
    public async Task<IActionResult> ConfirmEmailChange(string token)
    {
        var client = httpClientFactory.CreateClient(ApiClients.Users);
        var encodedToken = Uri.EscapeDataString(token);
        var response = await client.GetAsync($"/api/users/confirm-email-change?token={encodedToken}");

        return View(response.IsSuccessStatusCode ? "ConfirmEmailSuccess" : "ConfirmEmailError");
    }
    
    [HttpGet]
    public IActionResult ResendConfirmation() => View();

    [HttpPost]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = httpClientFactory.CreateClient(ApiClients.Users);
        var json = JsonSerializer.Serialize(model.Email);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await client.PostAsync("/api/users/resend-confirmation", content);

        TempData["Message"] = "If that email exists and is unconfirmed, a new link has been sent.";
        return RedirectToAction("Login", "Login");
    }
}