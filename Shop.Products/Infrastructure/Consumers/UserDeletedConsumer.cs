using MassTransit;
using Shop.Products.Application.Interfaces;
using Shop.Shared.Messages;

namespace Shop.Products.Infrastructure.Consumers;

public class UserDeletedConsumer(IProductExternalServices productExternalServices)
    : IConsumer<UserDeletedEvent>
{
    public Task Consume(ConsumeContext<UserDeletedEvent> context) =>
        productExternalServices.DeleteAllProductsForUser(context.Message.UserId);
}

