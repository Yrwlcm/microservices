namespace NotificationService.Infrastructure.Services;

public interface INotificationService
{
	Task SendOrderCreatedAsync(Guid orderId, decimal amount, List<string> items);
	Task SendStockReservedAsync(Guid orderId, bool success, string? reason);
	Task SendPaymentCompletedAsync(Guid orderId, bool success, string transactionId);
	Task SendOrderCompletedAsync(Guid orderId, DateTime estimatedDelivery);
	Task SendOrderFailedAsync(Guid orderId, string reason, string step);
}