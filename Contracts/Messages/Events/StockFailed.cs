namespace Contracts.Messages.Events;

public record StockFailed(Guid OrderId) : IEvent;