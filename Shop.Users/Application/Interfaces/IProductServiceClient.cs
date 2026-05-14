namespace Shop.Users.Application.Interfaces;

public interface IProductServiceClient
{
    Task DeactivateUserProducts(Guid userId);
    Task ReactivateUserProducts(Guid userId);
    Task DeleteUserProducts(Guid userId);
}