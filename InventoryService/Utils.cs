using Contracts.Messages.Events;
using InventoryService.Dto;
using Outbox;
using Outbox.Extensions;

namespace InventoryService;

public static class Utils
{
    public static GoodQuantityDto OrderItemToGoodItem(OrderItem orderItem) =>
        new(
            orderItem.Sku,
            orderItem.Quantity);
}