using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Products.Domain.Models;
using Shop.Products.Infrastructure.Data;

namespace Shop.Products.Tests.Integration;

public class ProductsSyncControllerTests(ProductsApiFactory factory) : IClassFixture<ProductsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ResetDb()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        db.Products.RemoveRange(db.Products);
        db.SaveChanges();
    }

    private void SeedProducts(Guid userId, int count = 2)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        for (var i = 0; i < count; i++)
        {
            db.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                Description = "Description for testing purposes",
                Price = 10m,
                IsAvailable = true,
                IsDeleted = false,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }
        db.SaveChanges();
    }

    private string GetInternalKey()
    {
        using var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IConfiguration>()["InternalApiKey"]!;
    }

    private void SetInternalKey() =>
        _client.DefaultRequestHeaders.Add("X-Internal-Key", GetInternalKey());

    private void ClearInternalKey() =>
        _client.DefaultRequestHeaders.Remove("X-Internal-Key");


    #region POST /api/products/users/{id}/deactivate

    [Fact]
    public async Task DeactivateUser_WithValidKey_Returns200()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        SeedProducts(userId);
        SetInternalKey();

        var response = await _client.PostAsync($"/api/products/users/{userId}/deactivate", null);
        ClearInternalKey();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_WithoutKey_Returns401()
    {
        ResetDb();

        var response = await _client.PostAsync($"/api/products/users/{Guid.NewGuid()}/deactivate", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_WithWrongKey_Returns401()
    {
        ResetDb();
        _client.DefaultRequestHeaders.Add("X-Internal-Key", "wrong-key");

        var response = await _client.PostAsync($"/api/products/users/{Guid.NewGuid()}/deactivate", null);
        _client.DefaultRequestHeaders.Remove("X-Internal-Key");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion


    #region POST /api/products/users/{id}/reactivate

    [Fact]
    public async Task ReactivateUser_WithValidKey_Returns200()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        SeedProducts(userId);
        SetInternalKey();

        var response = await _client.PostAsync($"/api/products/users/{userId}/reactivate", null);
        ClearInternalKey();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReactivateUser_WithoutKey_Returns401()
    {
        ResetDb();

        var response = await _client.PostAsync($"/api/products/users/{Guid.NewGuid()}/reactivate", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReactivateUser_WithWrongKey_Returns401()
    {
        ResetDb();
        _client.DefaultRequestHeaders.Add("X-Internal-Key", "wrong-key");

        var response = await _client.PostAsync($"/api/products/users/{Guid.NewGuid()}/reactivate", null);
        _client.DefaultRequestHeaders.Remove("X-Internal-Key");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion


    #region DELETE /api/products/users/{id}/delete

    [Fact]
    public async Task DeleteAllUserProducts_WithValidKey_Returns204()
    {
        ResetDb();
        var userId = Guid.NewGuid();
        SeedProducts(userId);
        SetInternalKey();

        var response = await _client.DeleteAsync($"/api/products/users/{userId}/delete");
        ClearInternalKey();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAllUserProducts_WithoutKey_Returns401()
    {
        ResetDb();

        var response = await _client.DeleteAsync($"/api/products/users/{Guid.NewGuid()}/delete");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAllUserProducts_WithWrongKey_Returns401()
    {
        ResetDb();
        _client.DefaultRequestHeaders.Add("X-Internal-Key", "wrong-key");

        var response = await _client.DeleteAsync($"/api/products/users/{Guid.NewGuid()}/delete");
        _client.DefaultRequestHeaders.Remove("X-Internal-Key");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}

