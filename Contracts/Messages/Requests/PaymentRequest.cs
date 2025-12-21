namespace Contracts.Messages.Requests;

public record PaymentRequest(Guid AccountId, Guid OrderId, decimal OrderPrice) : IRequest;