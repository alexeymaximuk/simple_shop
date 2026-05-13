using Microsoft.EntityFrameworkCore;
using Shop.Products.Application.DTOs;
using Shop.Products.Application.Interfaces;
using Shop.Products.Domain.Models;

namespace Shop.Products.Infrastructure.Data;

public class ProductRepository(ProductsDbContext dbContext) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id) =>
        await dbContext.Products.FindAsync(id);

    public Task<Product?> GetByIdNotDeletedAsync(Guid id) =>
        dbContext.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

    public async Task<IEnumerable<Product>> GetFilteredAsync(ProductFilterDto filter, bool showUnavailable)
    {
        var query = dbContext.Products.Where(p => !p.IsDeleted).AsQueryable();

        if (filter.Name != null)
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{filter.Name}%"));

        if (filter.UserId != null)
            query = query.Where(x => x.UserId == filter.UserId);

        if (filter.MaxPrice != null)
            query = query.Where(x => x.Price <= filter.MaxPrice);

        if (filter.MinPrice != null)
            query = query.Where(x => x.Price >= filter.MinPrice);

        if (!showUnavailable)
            query = query.Where(x => x.IsAvailable);

        return await query.ToListAsync();
    }

    public void Add(Product product) => dbContext.Products.Add(product);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();

    public async Task SoftDeleteByUserIdAsync(Guid userId)
    {
        var products = await dbContext.Products.Where(p => p.UserId == userId).ToListAsync();
        foreach (var p in products) p.IsDeleted = true;
        await dbContext.SaveChangesAsync();
    }

    public async Task RestoreByUserIdAsync(Guid userId)
    {
        var products = await dbContext.Products.Where(p => p.UserId == userId).ToListAsync();
        foreach (var p in products) p.IsDeleted = false;
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAllByUserIdAsync(Guid userId)
    {
        var products = await dbContext.Products.Where(p => p.UserId == userId).ToListAsync();
        dbContext.Products.RemoveRange(products);
        await dbContext.SaveChangesAsync();
    }
}

