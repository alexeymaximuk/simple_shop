using Microsoft.EntityFrameworkCore;
using Shop.Products.Domain.Models;

namespace Shop.Products.Infrastructure.Data;

public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.UserId);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).IsRequired();
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
        });
    }
}