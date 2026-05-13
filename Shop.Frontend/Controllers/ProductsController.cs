using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shop.Frontend.Constants;
using Shop.Frontend.Models;
using Shop.Shared.Controllers;

namespace Shop.Frontend.Controllers;

public class ProductsController(IHttpClientFactory httpClientFactory) : BaseController
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private HttpClient AuthorizedClient()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var client = httpClientFactory.CreateClient(ApiClients.Products);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ProductFilterViewModel filter)
    {
        var client = httpClientFactory.CreateClient(ApiClients.Products);
        var query = $"products/all?Name={filter.Name}&MinPrice={filter.MinPrice}&MaxPrice={filter.MaxPrice}";
        var response = await client.GetAsync(query);

        var products = new List<ProductViewModel>();
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            products = JsonSerializer.Deserialize<List<ProductViewModel>>(json, _jsonOptions) ?? [];
        }

        ViewBag.Filter = filter;
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> MyProducts(bool showDeleted = false)
    {
        var client = AuthorizedClient();
        var response = await client.GetAsync($"products/my-products?showDeleted={showDeleted}");

        var products = new List<ProductViewModel>();
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            products = JsonSerializer.Deserialize<List<ProductViewModel>>(json, _jsonOptions) ?? [];
        }

        ViewBag.ShowDeleted = showDeleted;
        return View(products);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = AuthorizedClient();
        var json = JsonSerializer.Serialize(new { model.Name, model.Description, model.Price });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("products", content);
        
        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Product created.";
            return RedirectToAction("MyProducts");
        }
        
        await AddApiErrors(response);
        
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var client = httpClientFactory.CreateClient(ApiClients.Products);
        var response = await client.GetAsync($"products/{id}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Failed to load product.";
            return RedirectToAction("MyProducts");
        }

        var json = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductViewModel>(json, _jsonOptions)!;

        return View(new ProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProductEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = AuthorizedClient();
        var json = JsonSerializer.Serialize(new { model.Name, model.Description, model.Price });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"products/{model.Id}", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Product updated.";
            return RedirectToAction("MyProducts");
        }
        
        await AddApiErrors(response);
        
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var client = AuthorizedClient();
        await client.PostAsync($"products/{id}/deactivate", null);
        return RedirectToAction("MyProducts");
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id)
    {
        var client = AuthorizedClient();
        await client.PostAsync($"products/{id}/activate", null);
        return RedirectToAction("MyProducts");
    }
}