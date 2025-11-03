using Rebus.Bus;

namespace OrderService.Models.Orders;

public class OrderPublisher(IBus bus, OrderMessageFactory messageFactory, ILogger<OrderPublisher> logger) : IOrderPublisher
{
    public async Task PublishAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        var orderCreated = OrderMessageFactory.Create(request);

        logger.LogInformation("Publishing order {OrderId} with {ItemCount} item(s)", request.OrderId, orderCreated.Items.Count);

        cancellationToken.ThrowIfCancellationRequested();
        await bus.Publish(orderCreated);
    }
}
