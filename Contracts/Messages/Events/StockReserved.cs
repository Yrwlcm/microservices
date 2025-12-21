
namespace Contracts.Messages.Events;

public record StockReserved(Guid OrderId, decimal OrderPrice) : IEvent;