namespace Contracts.Messages.Events;

public record StockReleaseFailed(Guid OrderId) : IEvent;