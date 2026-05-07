using Shop.Products.DTOs;
using Shop.Shared.Exceptions;

namespace Shop.Products.Services;

public partial class ProductService
{
    public async Task CreateProduct(Guid userId, ProductCreateDto dto)
    {
        var product = CreateProduct(dto, userId);
        dbContext.Products.Add(product);
        
        await dbContext.SaveChangesAsync();
    }
    
    public async Task EditProduct(Guid productId, Guid userId, ProductChangeInfoDto dto)
    {
        var product = await FetchProduct(productId, userId);

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Description = dto.Description;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteProduct(Guid productId, Guid userId)
    {
        var product = await FetchProduct(productId, userId);

        product.IsDeleted = true;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeactivateProduct(Guid productId, Guid userId)
    {
        var product = await FetchProduct(productId, userId);
        if (!product.IsAvailable) throw new InvalidRequestException("Product is not available");

        product.IsAvailable = false;

        await dbContext.SaveChangesAsync();
    }

    public async Task ActivateProduct(Guid productId, Guid userId)
    {
        var product = await FetchProduct(productId, userId);
        if (product.IsAvailable) throw new InvalidRequestException("Product is available");

        product.IsAvailable = true;

        await dbContext.SaveChangesAsync();
    }
}