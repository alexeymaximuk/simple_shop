namespace Shop.Users.Application.Interfaces;

public interface IUserEventPublisher
{
    Task DeactivateUserProducts(Guid userId);
    Task ReactivateUserProducts(Guid userId);
    Task DeleteUserProducts(Guid userId);
}