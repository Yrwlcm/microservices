using Rebus.Bus;

namespace OrderService.Models.Orders;

public class OrderPublisher(IBus bus, ILogger<OrderPublisher> logger) : IOrderPublisher
{
    public async Task PublishAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        var reserveStockRequest = OrderMessageFactory.Create(request);

        logger.LogInformation("Requesting reserving items for order {OrderId} with {ItemCount} item(s)", request.OrderId, reserveStockRequest.Items.Count);

        cancellationToken.ThrowIfCancellationRequested();
        await bus.Publish(reserveStockRequest);
    }
}
