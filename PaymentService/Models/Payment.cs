using CSharpFunctionalExtensions;
using PaymentService.Dto.Payment;
using PaymentService.Enums;

namespace PaymentService.Models;

public class Payment 
{
    public PaymentId Id { get; private set; }
    public AccountId AccountId { get; private set; }
    public OrderId OrderId { get; private set; }
    public DateTime FinishedOnUtc { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public decimal OrderPrice { get; private set; }

    private Payment()
    {
        
    }

    public static Result<Payment> Create(CreatePaymentDto createPaymentDto)
    {
        if (createPaymentDto.FinishedOnUtc > DateTime.UtcNow)
            return Result.Failure<Payment>("Факт оплаты не мог произойти в будущем");
        if (createPaymentDto.OrderPrice < 0)
            return Result.Failure<Payment>("Отрицательная сумма оплаты");
        var payment = new Payment()
        {
            Id = new PaymentId(Guid.NewGuid()),
            OrderId = createPaymentDto.OrderId,
            AccountId = createPaymentDto.AccountId,
            FinishedOnUtc = createPaymentDto.FinishedOnUtc,
            PaymentStatus = createPaymentDto.PaymentStatus,
            OrderPrice = createPaymentDto.OrderPrice
        };
        return Result.Success(payment);
    }
}