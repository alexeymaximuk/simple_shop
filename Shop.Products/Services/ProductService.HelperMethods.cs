using System.Linq.Expressions;
using Shop.Products.DTOs;
using Shop.Products.Models;
using Shop.Shared.Exceptions;

namespace Shop.Products.Services;

public partial class ProductService
{
    private Product CreateProduct(ProductInfoDto dto, Guid userId)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsAvailable = true,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        return product;
    }

    private async Task<Product> FetchProduct(Guid productId, Guid userId)
    {
        var product = await dbContext.Products.FindAsync(productId);
        if (product == null) throw new NotFoundException("Product not found");
        if (product.UserId != userId) throw new ForbiddenException("Access denied");
        
        return product;
    }
    
    private static Expression<Func<Product,ProductResponseDto>> MapToResponseDto => 
        p => new ProductResponseDto
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