using PaymentService.Enums;

namespace PaymentService.Application.Mappers;

public static class EnumMapper
{
    public static string PaymentStatusToString(PaymentStatus paymentStatus) =>
        paymentStatus switch
        {
            PaymentStatus.LowBalanceFailure => "Ошибка: недостаточно средств на балансе",
            PaymentStatus.MissingAccountFailure => "Ошибка: несуществующий счет",
            PaymentStatus.BalanceError => "Произошла ошибка при списании средств со счета",
            PaymentStatus.Success => "Оплачено",
            _ => throw new ArgumentException($"Неопределенное значение {nameof(PaymentStatus)}")
        };
}