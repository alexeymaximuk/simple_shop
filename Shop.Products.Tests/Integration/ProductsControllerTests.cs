using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Products.Application.DTOs;
using Shop.Products.Domain.Models;
using Shop.Products.Infrastructure.Data;

namespace Shop.Products.Tests.Integration;

public class ProductsControllerTests(ProductsApiFactory factory) : IClassFixture<ProductsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly ProductInfoDto ValidDto = new()
    {
        Name = "Test Product",
        Description = "A valid description that is long enough",
        Price = 9.99m
    };

    private void ResetDb()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        db.Products.RemoveRange(db.Products);
        db.SaveChanges();
    }

    private Guid SeedProduct(Guid userId, bool isAvailable = true)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Seeded Product",
            Description = "Seeded description for testing purposes",
            Price = 5m,
            IsAvailable = isAvailable,
            IsDeleted = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        db.Products.Add(product);
        db.SaveChanges();
        return product.Id;
    }

    private void SetAuth(Guid userId, string role = "User")
    {
        using var scope = factory.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwt.Generate(config, userId, role));
    }

    private void ClearAuth() =>
        _client.DefaultRequestHeaders.Authorization = null;


    #region POST /api/products

    [Fact]
    public async Task AddNewProduct_ValidData_Returns201()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        SetAuth(userId);

        var response = await _client.PostAsJsonAsync("/api/products", ValidDto);
        ClearAuth();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddNewProduct_Unauthenticated_Returns401()
    {
        ResetDb();

        var response = await _client.PostAsJsonAsync("/api/products", ValidDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddNewProduct_InvalidData_Returns400()
    {
        ResetDb();
        SetAuth(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/products", new ProductInfoDto
        {
            Name = "X",
            Description = "too short",
            Price = -1m
        });
        ClearAuth();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion


    #region PUT /api/products/{productId}

    [Fact]
    public async Task EditProduct_ValidData_Returns200()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        var productId = SeedProduct(userId);
        SetAuth(userId);

        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", ValidDto);
        ClearAuth();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EditProduct_Unauthenticated_Returns401()
    {
        ResetDb();
        var productId = SeedProduct(Guid.NewGuid());

        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", ValidDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EditProduct_WrongUser_Returns403()
    {
        ResetDb();
        var productId = SeedProduct(Guid.NewGuid());
        SetAuth(Guid.NewGuid());

        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", ValidDto);
        ClearAuth();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EditProduct_NotFound_Returns404()
    {
        ResetDb();
        SetAuth(Guid.NewGuid());

        var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", ValidDto);
        ClearAuth();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EditProduct_InvalidData_Returns400()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        var productId = SeedProduct(userId);
        SetAuth(userId);

        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", new ProductInfoDto
        {
            Name = "X",
            Description = "short",
            Price = 0m
        });
        ClearAuth();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion


    #region POST /api/products/{productId}/deactivate

    [Fact]
    public async Task Deactivate_ValidRequest_Returns200()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        var productId = SeedProduct(userId, isAvailable: true);
        SetAuth(userId);

        var response = await _client.PostAsync($"/api/products/{productId}/deactivate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_Unauthenticated_Returns401()
    {
        ResetDb();
        var productId = SeedProduct(Guid.NewGuid());

        var response = await _client.PostAsync($"/api/products/{productId}/deactivate", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_WrongUser_Returns403()
    {
        ResetDb();
        var productId = SeedProduct(Guid.NewGuid(), isAvailable: true);
        SetAuth(Guid.NewGuid());

        var response = await _client.PostAsync($"/api/products/{productId}/deactivate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_AlreadyDeactivated_Returns422()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        var productId = SeedProduct(userId, isAvailable: false);
        SetAuth(userId);

        var response = await _client.PostAsync($"/api/products/{productId}/deactivate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_NotFound_Returns404()
    {
        ResetDb();
        SetAuth(Guid.NewGuid());

        var response = await _client.PostAsync($"/api/products/{Guid.NewGuid()}/deactivate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion


    #region POST /api/products/{productId}/activate

    [Fact]
    public async Task Activate_ValidRequest_Returns200()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        var productId = SeedProduct(userId, isAvailable: false);
        SetAuth(userId);

        var response = await _client.PostAsync($"/api/products/{productId}/activate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Activate_Unauthenticated_Returns401()
    {
        ResetDb();
        var productId = SeedProduct(Guid.NewGuid());

        var response = await _client.PostAsync($"/api/products/{productId}/activate", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Activate_WrongUser_Returns403()
    {
        ResetDb();
        var productId = SeedProduct(Guid.NewGuid(), isAvailable: false);
        SetAuth(Guid.NewGuid());

        var response = await _client.PostAsync($"/api/products/{productId}/activate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Activate_AlreadyActive_Returns422()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        var productId = SeedProduct(userId, isAvailable: true);
        SetAuth(userId);

        var response = await _client.PostAsync($"/api/products/{productId}/activate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Activate_NotFound_Returns404()
    {
        ResetDb();
        SetAuth(Guid.NewGuid());

        var response = await _client.PostAsync($"/api/products/{Guid.NewGuid()}/activate", null);
        ClearAuth();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion


    #region GET /api/products/{productId}

    [Fact]
    public async Task GetProduct_Found_Returns200()
    {
        ResetDb();
        var productId = SeedProduct(Guid.NewGuid());

        var response = await _client.GetAsync($"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProduct_NotFound_Returns422()
    {
        ResetDb();

        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion


    #region GET /api/products/all

    [Fact]
    public async Task GetAllProducts_Returns200()
    {
        ResetDb();
        SeedProduct(Guid.NewGuid());

        var response = await _client.GetAsync("/api/products/all");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion


    #region GET /api/products/my-products

    [Fact]
    public async Task GetMyProducts_Authenticated_Returns200()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        SeedProduct(userId);
        SetAuth(userId);

        var response = await _client.GetAsync("/api/products/my-products");
        ClearAuth();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProducts_Unauthenticated_Returns401()
    {
        ResetDb();

        var response = await _client.GetAsync("/api/products/my-products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}

