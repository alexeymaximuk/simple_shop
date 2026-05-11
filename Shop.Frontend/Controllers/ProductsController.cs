using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shop.Frontend.Constants;
using Shop.Frontend.Models;

namespace Shop.Frontend.Controllers;

public class ProductsController(IHttpClientFactory httpClientFactory) : Controller
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
        var query = $"/api/products/all?Name={filter.Name}&MinPrice={filter.MinPrice}&MaxPrice={filter.MaxPrice}";
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
        var response = await client.GetAsync($"/api/products/my-products?showDeleted={showDeleted}");

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
        var response = await client.PostAsync("/api/products", content);
        
        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Product created.";
            return RedirectToAction("MyProducts");
        }

        ModelState.AddModelError("", "Failed to create product.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var client = httpClientFactory.CreateClient(ApiClients.Products);
        var response = await client.GetAsync($"/api/products/{id}");

        if (!response.IsSuccessStatusCode) return RedirectToAction("MyProducts");

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

        var response = await client.PutAsync($"/api/products/{model.Id}", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Product updated.";
            return RedirectToAction("MyProducts");
        }

        ModelState.AddModelError("", "Failed to update product.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var client = AuthorizedClient();
        await client.PostAsync($"/api/products/{id}/deactivate", null);
        return RedirectToAction("MyProducts");
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id)
    {
        var client = AuthorizedClient();
        await client.PostAsync($"/api/products/{id}/activate", null);
        return RedirectToAction("MyProducts");
    }
}