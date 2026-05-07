using Microsoft.EntityFrameworkCore;
using Shop.Products.Models;

namespace Shop.Products.Data;

public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}