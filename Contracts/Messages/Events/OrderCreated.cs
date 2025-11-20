namespace Contracts.Messages.Events;

public record OrderCreated(Guid OrderId, IReadOnlyList<OrderItem> Items) : IEvent;

public record OrderItem(string Sku, int Quantity);
