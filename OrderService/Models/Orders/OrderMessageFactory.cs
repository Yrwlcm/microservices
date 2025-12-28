using Contracts.Messages.Events;
using Contracts.Messages.Requests;

namespace OrderService.Models.Orders;

public class OrderMessageFactory
{
    public static ReserveStockRequest Create(OrderRequest request)
    {
        var items = request.Items
            .Select(item => new OrderItem(item.Sku, item.Quantity))
            .ToList();

        return new ReserveStockRequest(request.OrderId, items);
    }

    public static OrderCreatedNotification CreatedNotification(ReserveStockRequest request) => new()
    {
        OrderId = request.OrderId,
        Amount = request.Items.Sum(i => i.Quantity),
        Items = request.Items.Select(i => $"{i.Sku} (x{i.Quantity})").ToList()
    };
}
