namespace Contracts.Messages;

public sealed record OrderCreated(Guid OrderId, IReadOnlyList<OrderItem> Items);

public sealed record OrderItem(string Sku, int Qty);
