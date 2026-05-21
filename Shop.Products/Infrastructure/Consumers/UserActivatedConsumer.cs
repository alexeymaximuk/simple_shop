using MassTransit;
using Shop.Products.Application.Interfaces;
using Shop.Shared.Messages;

namespace Shop.Products.Infrastructure.Consumers;

public class UserActivatedConsumer(IProductExternalServices productExternalServices)
    : IConsumer<UserActivatedEvent>
{
    public Task Consume(ConsumeContext<UserActivatedEvent> context) =>
        productExternalServices.RestoreUserProducts(context.Message.UserId);
}

