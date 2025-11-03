namespace Contracts.Messages;

public record OrderCreated(Guid OrderId, IReadOnlyList<OrderItem> Items);

public record OrderItem(string Sku, int Quantity);
