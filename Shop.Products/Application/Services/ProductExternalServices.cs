using Shop.Products.Application.Interfaces;

namespace Shop.Products.Application.Services;

public class ProductExternalServices(IProductRepository productRepository) : IProductExternalServices
{
    public Task SoftDeleteUserProducts(Guid userId) =>
        productRepository.SoftDeleteByUserIdAsync(userId);

    public Task RestoreUserProducts(Guid userId) =>
        productRepository.RestoreByUserIdAsync(userId);

    public Task DeleteAllProductsForUser(Guid userId) =>
        productRepository.DeleteAllByUserIdAsync(userId);
}