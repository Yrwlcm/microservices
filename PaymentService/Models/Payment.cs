using CSharpFunctionalExtensions;
using PaymentService.Dto.Payment;
using PaymentService.Enums;

namespace PaymentService.Models;

public class Payment 
{
    public PaymentId Id { get; private set; }
    public AccountId AccountId { get; private set; }
    public OrderId OrderId { get; private set; }
    public DateTime AttemptedOnUtc { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }

    private Payment()
    {
        
    }

    public static Result<Payment> Create(CreatePaymentDto createPaymentDto)
    {
        if (createPaymentDto.AttemptedOnUtc > DateTime.UtcNow)
            return Result.Failure<Payment>("Факт оплаты не мог произойти в будущем");
        var payment = new Payment()
        {
            Id = new PaymentId(Guid.NewGuid()),
            OrderId = createPaymentDto.OrderId,
            AccountId = createPaymentDto.AccountId,
            AttemptedOnUtc = createPaymentDto.AttemptedOnUtc,
            PaymentStatus = createPaymentDto.PaymentStatus
        };
        return Result.Success(payment);
    }
}