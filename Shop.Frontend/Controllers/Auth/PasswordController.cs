using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shop.Frontend.Constants;
using Shop.Frontend.Models;
using Shop.Shared.Controllers;

namespace Shop.Frontend.Controllers.Auth;

public class PasswordController(IHttpClientFactory httpClientFactory) : BaseController
{
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        var client = httpClientFactory.CreateClient(ApiClients.Users);
        var json =  JsonSerializer.Serialize(new {email  = model.Email});
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await client.PostAsync("/api/users/reset-password-request", content);
        
        TempData["Message"] = "If user with that email exists, a reset link will be send";
        return RedirectToAction("Login", "Login");
    }

    [HttpGet]
    public IActionResult ResetPassword(string token) => View(new ResetPasswordViewModel{Token = token});
    
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var client = httpClientFactory.CreateClient(ApiClients.Users);
        var json = JsonSerializer.Serialize(new {token = model.Token, password = model.Password});
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/users/reset-password", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Password reset successful";
            return RedirectToAction("Login", "Login");
        }
        
        await AddApiErrors(response);
        
        return View(model);
    }
}