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

    public Task SoftDeleteByUserIdAsync(Guid userId) =>
        dbContext.Products
            .Where(p => p.UserId == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDeleted, true));

    public Task RestoreByUserIdAsync(Guid userId) =>
        dbContext.Products
            .Where(p => p.UserId == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDeleted, false));

    public Task DeleteAllByUserIdAsync(Guid userId) =>
        dbContext.Products
            .Where(p => p.UserId == userId)
            .ExecuteDeleteAsync();
}

