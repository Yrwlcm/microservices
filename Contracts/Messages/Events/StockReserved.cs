namespace Contracts.Messages;

public record StockReserved(Guid OrderId, IReadOnlyList<OrderItem> ReservedItems, decimal OrderPrice) : IEvent;