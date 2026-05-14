using NSubstitute;
using Shop.Products.Application.DTOs;
using Shop.Products.Application.Interfaces;
using Shop.Products.Application.Services;
using Shop.Products.Domain.Models;
using Shop.Shared.Exceptions;

namespace Shop.Products.Tests.Services;

public class ProductCommandServiceTest
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    
    
    private readonly ProductCommandService _sut;

    public ProductCommandServiceTest()
    {
        _sut = new ProductCommandService(_productRepository);
    }

    #region CreateProduct
    
    [Fact]
    public async Task CreateProduct_ValidData_CreatesProduct()
    {
        var userId = Guid.NewGuid();
        var dto = new ProductInfoDto
        {
            Name = "Test name",
            Description = "Test description",
            Price = 1
        };

        var productId = await _sut.CreateProduct(userId, dto);

        _productRepository.Received(1).Add(Arg.Is<Product>(p =>
            p.Id == productId &&
            p.Name == dto.Name &&
            p.Description == dto.Description &&
            p.Price == dto.Price &&
            p.UserId == userId));
        await _productRepository.Received(1).SaveChangesAsync();
    }
    
    #endregion
    
    
    #region EditProduct
    
    [Fact]
    public async Task EditProduct_ProductDoesntExist_ThrowsNotFoundException()
    {
        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns((Product?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.EditProduct(Guid.NewGuid(), Guid.NewGuid(), new ProductInfoDto { Name = "Test name", Description = "Test description", Price = 1 }));
    }
    
    [Fact]
    public async Task EditProduct_WrongUser_ThrowsForbiddenException()
    {
        var userId1 = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            UserId = userId1,
            Name = "Test name",
            Description = "Test description"
        };

        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns(product);
        
        var userId2 = Guid.NewGuid();
        
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.EditProduct(product.Id, userId2, new ProductInfoDto { Name = "Test name", Description = "Test description", Price = 1 }));
    }
    
    [Fact]
    public async Task EditProduct_CorrectRequest_ShouldEditProduct()
    {
        var userId = Guid.NewGuid();
        var productGuid = Guid.NewGuid();
        
        var product = new Product
        {
            Id = productGuid,
            UserId = userId,
            Name = "Old name",
            Description = "Old Description",
            Price = 1
        };

        var dto = new ProductInfoDto
        {
            Name = "New name",
            Description = "New Description",
            Price = 2
        };
        
        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns(product);

        await _sut.EditProduct(product.Id, userId, dto);

        Assert.Equal(dto.Name, product.Name);
        Assert.Equal(dto.Description, product.Description);
        Assert.Equal(dto.Price, product.Price);
        Assert.Equal(userId, product.UserId);
        Assert.Equal(productGuid, product.Id);
        await _productRepository.Received(1).SaveChangesAsync();
    }
    
    #endregion
    
    
    #region ActivateProduct
    
    [Fact]
    public async Task ActivateProduct_ProductDoesntExist_ThrowsNotFoundException()
    {
        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns((Product?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ActivateProduct(Guid.NewGuid(), Guid.NewGuid()));
    }
    
    [Fact]
    public async Task ActivateProduct_WrongUser_ThrowsForbiddenException()
    {
        var userId1 = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            UserId = userId1,
            Name = "Test name",
            Description = "Test description"
        };

        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns(product);
        
        var userId2 = Guid.NewGuid();
        
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.ActivateProduct(product.Id, userId2));
    }
    
    [Fact]
    public async Task ActivateProduct_ProductAlreadyActivated_ThrowsInvalidRequestException()
    {
        var userId = Guid.NewGuid();
        var product = new Product
        {
            IsAvailable = true,
            UserId = userId,
            Name = "Test name",
            Description = "Test description"
        };
        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns(product);
        
        await Assert.ThrowsAsync<InvalidRequestException>(()=>
            _sut.ActivateProduct(Guid.NewGuid(), userId));
    }
    
    [Fact]
    public async Task ActivateProduct_CorrectRequest_ActivatesProduct()
    {
        var userId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsAvailable = false,
            Name = "Test name",
            Description = "Test description"
        };

        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns(product);
        
        await _sut.ActivateProduct(product.Id, userId);
        
        Assert.True(product.IsAvailable);
        await _productRepository.Received(1).SaveChangesAsync();
    }
    
    #endregion
    

    #region DeactivateProduct
    
    [Fact]
    public async Task DeactivateProduct_ProductDoesntExist_ThrowsNotFoundException()
    {
        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns((Product?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.DeactivateProduct(Guid.NewGuid(), Guid.NewGuid()));
    }
    
    [Fact]
    public async Task DeactivateProduct_WrongUser_ThrowsForbiddenException()
    {
        var userId1 = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            UserId = userId1,
            Name = "Test name",
            Description = "Test description"
        };

        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns(product);
        
        var userId2 = Guid.NewGuid();
        
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.DeactivateProduct(product.Id, userId2));
    }
    
    [Fact]
    public async Task DeactivateProduct_ProductAlreadyDeactivated_ThrowsInvalidRequestException()
    {
        var userId = Guid.NewGuid();
        var product = new Product
        {
            IsAvailable = false,
            UserId = userId,
            Name = "Test name",
            Description = "Test description"
        };
        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns(product);
        
        await Assert.ThrowsAsync<InvalidRequestException>(()=>
            _sut.DeactivateProduct(Guid.NewGuid(), userId));
    }
    
    [Fact]
    public async Task DeactivateProduct_CorrectRequest_DeactivatesProduct()
    {
        var userId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsAvailable = true,
            Name = "Test name",
            Description = "Test description"
        };

        _productRepository.GetByIdNotDeletedAsync(Arg.Any<Guid>()).Returns(product);
        
        await _sut.DeactivateProduct(product.Id, userId);
        
        Assert.False(product.IsAvailable);
        await _productRepository.Received(1).SaveChangesAsync();
    }

    #endregion
}