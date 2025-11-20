namespace Contracts.Messages;

public record StockFailed(Guid OrderId, IReadOnlyList<OrderItem> Items) : IEvent;