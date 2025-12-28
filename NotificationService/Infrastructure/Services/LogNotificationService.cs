namespace NotificationService.Infrastructure.Services;

public class LogNotificationService(ILogger<LogNotificationService> logger) : INotificationService
{
	public Task SendOrderCreatedAsync(Guid orderId, decimal amount, List<string> items)
	{
		logger.LogInformation("Уведомление: Заказ {OrderId} создан. Количество: {Amount:C}. Товары: {Items}", orderId, amount, string.Join(", ", items));
		return Task.CompletedTask;
	}

	public Task SendStockReservedAsync(Guid orderId, bool success, string? reason)
	{
		if (success)
			logger.LogInformation("Уведомление: Товары успешно зарезервироны для заказа {OrderId}", orderId);
		else
			logger.LogWarning("Уведомление: Не удалось зарезервировать товары для заказа {OrderId}. Причина: {Reason}", orderId, reason);

		return Task.CompletedTask;
	}

	public Task SendPaymentCompletedAsync(Guid orderId, bool success, string transactionId)
	{
		if (success)
			logger.LogInformation("Уведомление: Оплата заказа {OrderId} проведена успешно. Транзакция: {TransactionId}", orderId, transactionId);
		else
			logger.LogWarning("Уведомление: Не удалось оплатить заказ {OrderId}. Транзакция: {TransactionId}", orderId, transactionId);

		return Task.CompletedTask;
	}

	public Task SendOrderCompletedAsync(Guid orderId, DateTime estimatedDelivery)
	{
		logger.LogInformation("Уведомление: Заказ {OrderId} успешно завершен. Дата окончания: {DeliveryDate}", orderId, estimatedDelivery.ToString("yyyy-MM-dd"));
		return Task.CompletedTask;
	}

	public Task SendOrderFailedAsync(Guid orderId, string reason, string step)
	{
		logger.LogError("Уведомление: Ошибка в заказе {OrderId} на шаге '{Step}'. Причина: {Reason}", orderId, step, reason);
		return Task.CompletedTask;
	}
}