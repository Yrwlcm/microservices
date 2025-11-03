using Contracts.Messages;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace InventoryWorker;

public class OrderCreatedHandler(ILogger<OrderCreatedHandler> logger, Random random)
    : IHandleMessages<OrderCreated>
{
    public async Task Handle(OrderCreated message)
    {
        logger.LogInformation("Received order {OrderId} with {ItemCount} item(s)", message.OrderId, message.Items.Count);

        foreach (var item in message.Items)
        {
            logger.LogInformation(" -> SKU {Sku}, quantity {Quantity}", item.Sku, item.Quantity);
        }

        var delay = random.Next(100, 301);
        await Task.Delay(delay);

        logger.LogInformation("Order {OrderId} processed in {Delay} ms", message.OrderId, delay);
    }
}