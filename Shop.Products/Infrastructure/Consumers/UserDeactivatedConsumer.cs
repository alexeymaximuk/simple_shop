using MassTransit;
using Shop.Products.Application.Interfaces;
using Shop.Shared.Messages;

namespace Shop.Products.Infrastructure.Consumers;

public class UserDeactivatedConsumer(IProductExternalServices productExternalServices)
    : IConsumer<UserDeactivatedEvent>
{
    public Task Consume(ConsumeContext<UserDeactivatedEvent> context) =>
        productExternalServices.SoftDeleteUserProducts(context.Message.UserId);
}

