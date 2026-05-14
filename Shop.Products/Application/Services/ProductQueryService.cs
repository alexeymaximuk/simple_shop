using Shop.Products.Application.DTOs;
using Shop.Products.Application.Interfaces;
using Shop.Shared.Exceptions;

namespace Shop.Products.Application.Services;

public class ProductQueryService(IProductRepository productRepository) : IProductQueryService
{
    public async Task<IEnumerable<ProductResponseDto>> GetAllProducts(ProductFilterDto filter, bool showUnavailable = false)
    {
        var products = await productRepository.GetFilteredAsync(filter, showUnavailable);
        return products.Select(MapToResponseDto);
    }

    public async Task<ProductResponseDto> GetProductById(Guid productId)
    {
        var product = await productRepository.GetByIdNotDeletedAsync(productId);
        if (product == null) throw new NotFoundException("Product is not found");

        return MapToResponseDto(product);
    }

    private static ProductResponseDto MapToResponseDto(Domain.Models.Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        CreateTime = p.CreatedAt,
        UserId = p.UserId,
        Description = p.Description,
        IsDeleted = p.IsDeleted,
        IsActive = p.IsAvailable
    };
}