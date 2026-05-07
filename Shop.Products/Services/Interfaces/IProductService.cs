using Microsoft.AspNetCore.Mvc;
using Shop.Products.DTOs;

namespace Shop.Products.Services.Interfaces;

public interface IProductService
{ 
    Task CreateProduct(Guid userId, ProductCreateDto dto);
    Task EditProduct(Guid productId, Guid userId, ProductChangeInfoDto dto);
    Task DeleteProduct(Guid productId, Guid userId);
    Task DeactivateProduct(Guid productId, Guid userId);
    Task ActivateProduct(Guid productId, Guid userId);

    Task<IEnumerable<ProductResponseDto>> GetAllProducts(ProductFilterDto filter, bool showUnavailable = false);
    Task<ProductResponseDto> GetProductById(Guid productId);
}