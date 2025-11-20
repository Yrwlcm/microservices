namespace Contracts.Messages.Events;

public record PaymentSucceeded(Guid OrderId) : IEvent;