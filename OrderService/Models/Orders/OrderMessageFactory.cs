using Contracts.Messages;
using Contracts.Messages.Events;

namespace OrderService.Models.Orders;

public class OrderMessageFactory
{
    public static OrderCreated Create(OrderRequest request)
    {
        var items = request.Items
            .Select(item => new OrderItem(item.Sku, item.Quantity))
            .ToList();

        return new OrderCreated(request.OrderId, items);
    }
}
