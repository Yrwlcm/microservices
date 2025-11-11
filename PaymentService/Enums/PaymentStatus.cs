namespace PaymentService.Enums;

public enum PaymentStatus
{
    Success = 0,
    MissingAccountFailure = 1,
    LowBalanceFailure = 2,
    UnexpectedFailure = 3
}