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
}
