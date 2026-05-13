using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shop.Users.Application.Interfaces;
using Shop.Users.Infrastructure.Data;
using Shop.Users.Infrastructure.Email;

namespace Shop.Users.Tests.Integration;

public class UsersApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbDescriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<UsersDbContext>)).ToList();
            foreach (var d in dbDescriptors) services.Remove(d);

            services.AddDbContext<UsersDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            
            var emailDescriptors = services.Where(d =>
                d.ServiceType == typeof(IEmailSendingService)).ToList();
            foreach (var d in emailDescriptors) services.Remove(d);
            services.AddScoped(_ => Substitute.For<IEmailSendingService>());

            var productDescriptors = services.Where(d =>
                d.ServiceType == typeof(IProductServiceClient)).ToList();
            foreach (var d in productDescriptors) services.Remove(d);
            services.AddScoped(_ => Substitute.For<IProductServiceClient>());
        });

        builder.UseEnvironment("Testing");
    }
}