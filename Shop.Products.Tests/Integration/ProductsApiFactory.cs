using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shop.Products.Infrastructure.Data;

namespace Shop.Products.Tests.Integration;

public class ProductsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestProductsDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ProductsDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<ProductsDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }
}
