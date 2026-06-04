using Microsoft.EntityFrameworkCore;
using Shop.Products.Infrastructure.Data;

namespace Shop.Products.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        if (!app.Environment.IsEnvironment("Testing"))
            await db.Database.MigrateAsync();
    }
}