using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Users.Domain.Models;
using Shop.Users.Infrastructure.Data;
using Shop.Users.Infrastructure.Data.Seed;

namespace Shop.Users.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        
        if (!app.Environment.IsEnvironment("Testing"))
            await db.Database.MigrateAsync();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        await AdminSeeder.SeedAsync(db, passwordHasher, config);
    }
}