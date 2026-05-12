using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Shared.Controllers;

public abstract class BaseController : Controller
{
    protected async Task AddApiErrors(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var json = JsonDocument.Parse(body).RootElement;
        
            if (json.ValueKind == JsonValueKind.Array)
            {
                foreach (var error in json.EnumerateArray())
                    ModelState.AddModelError("", error.GetProperty("errorMessage").GetString()!);
            }
            else if (json.TryGetProperty("error", out var error))
            {
                ModelState.AddModelError("", error.GetString()!);
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong.");
            }
        }
        catch
        {
            ModelState.AddModelError("", "Something went wrong.");
        }
    }
}