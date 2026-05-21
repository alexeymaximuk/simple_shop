using MassTransit;
using Shop.Shared.Messages;
using Shop.Users.Application.Interfaces;

namespace Shop.Users.Infrastructure.Publishers;

public class UserEventPublisher(IPublishEndpoint publishEndpoint) : IUserEventPublisher
{
    public Task DeactivateUserProducts(Guid userId) =>
        publishEndpoint.Publish(new UserDeactivatedEvent(userId));

    public Task ReactivateUserProducts(Guid userId) =>
        publishEndpoint.Publish(new UserActivatedEvent(userId));

    public Task DeleteUserProducts(Guid userId) =>
        publishEndpoint.Publish(new UserDeletedEvent(userId));
}