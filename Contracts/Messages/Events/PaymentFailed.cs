namespace Contracts.Messages;

public record PaymentFailed(Guid OrderId, IReadOnlyList<OrderItem> Items) : IEvent;