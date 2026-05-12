using Microsoft.EntityFrameworkCore;
using Shop.Users.Models;

namespace Shop.Users.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Name).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
        });
    }
}