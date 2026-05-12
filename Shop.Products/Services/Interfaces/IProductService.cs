using Microsoft.AspNetCore.Mvc;
using Shop.Products.DTOs;

namespace Shop.Products.Services.Interfaces;

public interface IProductService
{
    Task<Guid> CreateProduct(Guid userId, ProductInfoDto dto);
    Task EditProduct(Guid productId, Guid userId, ProductInfoDto dto);
    Task DeactivateProduct(Guid productId, Guid userId);
    Task ActivateProduct(Guid productId, Guid userId);

    Task<IEnumerable<ProductResponseDto>> GetAllProducts(ProductFilterDto filter, bool showUnavailable = false);
    Task<ProductResponseDto> GetProductById(Guid productId);
    
    Task SoftDeleteUserProducts(Guid userId);
    Task RestoreUserProducts(Guid userId);
    Task DeleteAllProductsForUser(Guid userId);
}