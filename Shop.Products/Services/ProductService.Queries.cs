using Microsoft.EntityFrameworkCore;
using Shop.Products.DTOs;
using Shop.Shared.Exceptions;

namespace Shop.Products.Services;

public partial class ProductService
{
    public async Task<IEnumerable<ProductResponseDto>> GetAllProducts(
        ProductFilterDto filter, bool showUnavailable = false
    )
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
            query = query.Where(x => x.IsAvailable == true);
        
        var product = await query.Select(MapToResponseDto).ToListAsync();

        return product;
    }

    public async Task<ProductResponseDto> GetProductById(Guid productId)
    {
        var product = await dbContext.Products.Where(x => x.Id == productId && !x.IsDeleted).Select(MapToResponseDto)
            .FirstOrDefaultAsync();
        if (product == null) throw new InvalidRequestException("Product is not found");

        return product;
    }
}