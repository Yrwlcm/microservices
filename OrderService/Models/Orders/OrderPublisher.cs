using Rebus.Bus;

namespace OrderService.Models.Orders;

public class OrderPublisher(IBus bus, ILogger<OrderPublisher> logger) : IOrderPublisher
{
    public async Task PublishAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        var reserveStockRequest = OrderMessageFactory.Create(request);
        var notification = OrderMessageFactory.CreatedNotification(reserveStockRequest);

        logger.LogInformation("Создан заказ {OrderId} с {ItemCount} позициями", request.OrderId, reserveStockRequest.Items.Count);

        cancellationToken.ThrowIfCancellationRequested();
        await bus.Publish(reserveStockRequest);
        await bus.Publish(notification);
    }
}
