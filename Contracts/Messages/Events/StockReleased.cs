namespace Contracts.Messages.Events;

public record StockReleased(Guid OrderId) : IEvent;