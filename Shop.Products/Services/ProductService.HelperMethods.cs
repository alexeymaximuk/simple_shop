using System.Linq.Expressions;
using Shop.Products.DTOs;
using Shop.Products.Models;
using Shop.Shared.Exceptions;

namespace Shop.Products.Services;

public partial class ProductService
{
    private Product CreateProduct(ProductCreateDto dto, Guid pserId)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsAvailable = true,
            UserId = pserId,
            CreatedAt = DateTime.UtcNow
        };
        return product;
    }

    private async Task<Product> FetchProduct(Guid productId, Guid pserId)
    {
        var product = await dbContext.Products.FindAsync(productId);
        if (product == null) throw new NotFoundException("Product not found");
        if (product.UserId != pserId) throw new ForbiddenException("Access denied");
        
        return product;
    }
    
    
    private static Expression<Func<Product,ProductResponseDto>> MapToResponseDto => 
        p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            IsDeleted = p.IsDeleted,
            IsActive = p.IsAvailable
        };
}