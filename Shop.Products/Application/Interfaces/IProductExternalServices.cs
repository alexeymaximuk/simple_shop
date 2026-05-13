namespace Shop.Products.Application.Interfaces;

public interface IProductExternalServices
{
    Task SoftDeleteUserProducts(Guid userId);
    Task RestoreUserProducts(Guid userId);
    Task DeleteAllProductsForUser(Guid userId);
}