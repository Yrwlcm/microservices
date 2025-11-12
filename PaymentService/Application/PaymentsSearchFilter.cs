namespace PaymentService.Application;

public record PaymentsSearchFilter(Guid? OrderId = null, Guid? AccountId = null, int Page = 1, int Limit = 100);