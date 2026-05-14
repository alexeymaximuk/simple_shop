using Shop.Products.Application.DTOs;

namespace Shop.Products.Application.Interfaces;

public interface IProductCommandService
{
    Task<Guid> CreateProduct(Guid userId, ProductInfoDto dto);
    Task EditProduct(Guid productId, Guid userId, ProductInfoDto dto);
    Task DeactivateProduct(Guid productId, Guid userId);
    Task ActivateProduct(Guid productId, Guid userId);
}