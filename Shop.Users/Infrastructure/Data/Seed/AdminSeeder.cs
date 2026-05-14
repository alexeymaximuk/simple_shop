using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Users.Domain.Models;

namespace Shop.Users.Infrastructure.Data.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(UsersDbContext db, IPasswordHasher<User> passwordHasher, IConfiguration config)
    {
        var email = config["Admin:AdminEmail"]!;
        var password = config["Admin:AdminPassword"]!;
        var role = config["Admin:AdminRole"]!;
        var name = config["Admin:AdminName"]!;

        var admin = await db.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (admin == null)
        {
            admin = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Role = role,
                Name = name,
                PasswordHash = string.Empty, // overwritten immediately by passwordHasher.HashPassword below
                IsEmailConfirmed = true,
                IsActive = true
            };
            
            admin.PasswordHash = passwordHasher.HashPassword(admin, password);

            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}