using Shop.Products.Application.DTOs;
using Shop.Products.Application.Interfaces;
using Shop.Products.Domain.Models;
using Shop.Shared.Exceptions;

namespace Shop.Products.Application.Services;

public class ProductCommandService(IProductRepository productRepository) : IProductCommandService
{
    public async Task<Guid> CreateProduct(Guid userId, ProductInfoDto dto)
    {
        var product = BuildProduct(dto, userId);
        productRepository.Add(product);
        await productRepository.SaveChangesAsync();
        return product.Id;
    }

    public async Task EditProduct(Guid productId, Guid userId, ProductInfoDto dto)
    {
        var product = await FetchProduct(productId, userId);

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Description = dto.Description;

        await productRepository.SaveChangesAsync();
    }

    public async Task DeactivateProduct(Guid productId, Guid userId)
    {
        var product = await FetchProduct(productId, userId);
        if (!product.IsAvailable) throw new InvalidRequestException("Product is not available");

        product.IsAvailable = false;
        await productRepository.SaveChangesAsync();
    }

    public async Task ActivateProduct(Guid productId, Guid userId)
    {
        var product = await FetchProduct(productId, userId);
        if (product.IsAvailable) throw new InvalidRequestException("Product is available");

        product.IsAvailable = true;
        await productRepository.SaveChangesAsync();
    }

    private static Product BuildProduct(ProductInfoDto dto, Guid userId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsAvailable = true,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

    private async Task<Product> FetchProduct(Guid productId, Guid userId)
    {
        var product = await productRepository.GetByIdAsync(productId);
        if (product == null) throw new NotFoundException("Product not found");
        if (product.UserId != userId) throw new ForbiddenException("Access denied");

        return product;
    }
}