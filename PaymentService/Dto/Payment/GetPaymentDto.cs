namespace PaymentService.Dto.Payment;

public record GetPaymentDto(Guid PaymentId, Guid AccountId, Guid OrderId, DateTime FinishedOnUtc, string PaymentStatus,
    decimal OrderPrice);