namespace Contracts.Messages.Events;

public record PaymentFailed(Guid OrderId) : IEvent;