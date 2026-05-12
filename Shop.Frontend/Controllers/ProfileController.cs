using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Shop.Frontend.Constants;
using Shop.Frontend.Models;
using Shop.Shared.Controllers;

namespace Shop.Frontend.Controllers;

public class ProfileController(IHttpClientFactory httpClientFactory) : BaseController
{
    public IActionResult MyProducts()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost("change-name")]
    public async Task<IActionResult> ChangeName(ChangeNameViewModel model)
    {
        if (!ModelState.IsValid) return View("Index", model);

        var token = HttpContext.Session.GetString("JwtToken");

        var client = httpClientFactory.CreateClient(ApiClients.Users);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(new { name = model.Name });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PatchAsync("users/me/change-name", content);

        if (response.IsSuccessStatusCode)
            TempData["Message"] = "Name updated successfully.";
        else
        {
            await AddApiErrors(response);
            return View("Index", model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        if (!ModelState.IsValid) return View("Index", model);

        var token = HttpContext.Session.GetString("JwtToken");

        var client = httpClientFactory.CreateClient(ApiClients.Users);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(new { newEmail = model.NewEmail });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("users/me/change-email", content);

        if (response.IsSuccessStatusCode)
            TempData["Message"] = "Confirmation sent to your new email.";
        else
        {
            await AddApiErrors(response);
            return View("Index", model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete()
    {
        var token = HttpContext.Session.GetString("JwtToken");

        var client = httpClientFactory.CreateClient(ApiClients.Users);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.DeleteAsync("/api/users/me");

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
} 