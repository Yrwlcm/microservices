namespace Contracts.Messages.Requests;

public record PaymentRequest(Guid OrderId, decimal OrderPrice) : IRequest;