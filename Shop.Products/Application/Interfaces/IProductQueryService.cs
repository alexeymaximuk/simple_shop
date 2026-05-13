using Shop.Products.Application.DTOs;

namespace Shop.Products.Application.Interfaces;

public interface IProductQueryService
{
    Task<ProductResponseDto> GetProductById(Guid productId);
    Task<IEnumerable<ProductResponseDto>> GetAllProducts(ProductFilterDto filter, bool showUnavailable);
}