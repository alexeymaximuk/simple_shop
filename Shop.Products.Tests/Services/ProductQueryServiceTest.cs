using NSubstitute;
using Shop.Products.Application.DTOs;
using Shop.Products.Application.Interfaces;
using Shop.Products.Application.Services;
using Shop.Products.Domain.Models;
using Shop.Shared.Exceptions;

namespace Shop.Products.Tests.Services;

public class ProductQueryServiceTest
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();

    private readonly ProductQueryService _sut;

    public ProductQueryServiceTest()
    {
        _sut = new ProductQueryService(_productRepository);
    }

    #region GetProductById

    [Fact]
    public async Task GetProductById_ProductNotFound_ThrowsNotFoundException()
    {
        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetProductById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetProductById_ProductFound_ReturnsMappedDto()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test name",
            Description = "Test description",
            Price = 1,
            IsAvailable = true,
            IsDeleted = false,
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        _productRepository.GetByIdNotDeletedAsync(product.Id).Returns(product);

        var result = await _sut.GetProductById(product.Id);

        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.Price, result.Price);
        Assert.Equal(product.UserId, result.UserId);
        Assert.Equal(product.IsAvailable, result.IsActive);
        Assert.Equal(product.IsDeleted, result.IsDeleted);
        Assert.Equal(product.CreatedAt, result.CreateTime);
    }

    #endregion


    #region GetAllProducts

    [Fact]
    public async Task GetAllProducts_EmptyResult_ReturnsEmptyCollection()
    {
        _productRepository.GetFilteredAsync(Arg.Any<ProductFilterDto>(), Arg.Any<bool>())
            .Returns([]);

        var result = await _sut.GetAllProducts(new ProductFilterDto());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllProducts_ReturnsAllMappedDtos()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "A", Description = "Desc A", Price = 1, UserId = Guid.NewGuid() },
            new() { Id = Guid.NewGuid(), Name = "B", Description = "Desc B", Price = 2, UserId = Guid.NewGuid() }
        };

        _productRepository.GetFilteredAsync(Arg.Any<ProductFilterDto>(), Arg.Any<bool>())
            .Returns(products);

        var result = (await _sut.GetAllProducts(new ProductFilterDto())).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(products[0].Id, result[0].Id);
        Assert.Equal(products[1].Id, result[1].Id);
    }

    [Fact]
    public async Task GetAllProducts_PassesFilterAndShowUnavailableToRepository()
    {
        var filter = new ProductFilterDto { Name = "Test", MinPrice = 5m, MaxPrice = 50m };
        _productRepository.GetFilteredAsync(Arg.Any<ProductFilterDto>(), Arg.Any<bool>())
            .Returns([]);

        await _sut.GetAllProducts(filter, showUnavailable: true);

        await _productRepository.Received(1).GetFilteredAsync(filter, true);
    }

    #endregion
}