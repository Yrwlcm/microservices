using PaymentService.Enums;
using PaymentService.Models;

namespace PaymentService.Dto.Payment;

public record CreatePaymentDto(AccountId AccountId, OrderId OrderId, DateTime AttemptedOnUtc, PaymentStatus PaymentStatus);