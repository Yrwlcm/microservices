namespace Contracts.Messages;

public record PaymentSucceeded(Guid OrderId) : IEvent;