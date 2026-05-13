using NSubstitute;
using Shop.Products.Application.Interfaces;
using Shop.Products.Application.Services;

namespace Shop.Products.Tests.Services;

public class ProductExternalServiceTest
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();

    private readonly ProductExternalServices _sut;

    public ProductExternalServiceTest()
    {
        _sut = new ProductExternalServices(_productRepository);
    }

    #region SoftDeleteUserProducts

    [Fact]
    public async Task SoftDeleteUserProducts_CallsRepository()
    {
        var userId = Guid.NewGuid();

        await _sut.SoftDeleteUserProducts(userId);

        await _productRepository.Received(1).SoftDeleteByUserIdAsync(userId);
    }

    #endregion
    

    #region RestoreUserProducts

    [Fact]
    public async Task RestoreUserProducts_CallsRepository()
    {
        var userId = Guid.NewGuid();

        await _sut.RestoreUserProducts(userId);

        await _productRepository.Received(1).RestoreByUserIdAsync(userId);
    }

    #endregion


    #region DeleteAllProductsForUser

    [Fact]
    public async Task DeleteAllProductsForUser_CallsRepository()
    {
        var userId = Guid.NewGuid();

        await _sut.DeleteAllProductsForUser(userId);

        await _productRepository.Received(1).DeleteAllByUserIdAsync(userId);
    }

    #endregion
}