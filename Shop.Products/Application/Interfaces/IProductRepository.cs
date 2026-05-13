using Shop.Products.Application.DTOs;
using Shop.Products.Domain.Models;

namespace Shop.Products.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetByIdNotDeletedAsync(Guid id);
    Task<IEnumerable<Product>> GetFilteredAsync(ProductFilterDto filter, bool showUnavailable);
    void Add(Product product);
    Task SaveChangesAsync();
    Task SoftDeleteByUserIdAsync(Guid userId);
    Task RestoreByUserIdAsync(Guid userId);
    Task DeleteAllByUserIdAsync(Guid userId);
}

