using Microsoft.EntityFrameworkCore;
using Shop.Products.DTOs;
using Shop.Shared.Exceptions;

namespace Shop.Products.Services;

public partial class ProductService
{
    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsForUser(Guid userId)
    {
        var product = await dbContext.Products.Where(x => x.UserId == userId).Select(MapToResponseDto).ToListAsync();

        return product;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllProducts()
    {
        var product = await dbContext.Products.Select(MapToResponseDto).ToListAsync();

        return product;
    }

    public async Task<ProductResponseDto> GetProductById(Guid productId)
    {
        var product = await dbContext.Products.Where(x => x.Id == productId).Select(MapToResponseDto)
            .FirstOrDefaultAsync();
        if (product == null) throw new InvalidRequestException("Product is not found");

        return product;
    }
}