using Contracts.Messages.Events;
using NotificationService.Infrastructure.Services;
using Rebus.Handlers;

namespace NotificationService.Infrastructure.Consumers;

public class PaymentCompletedConsumer(INotificationService notificationService, ILogger<PaymentCompletedConsumer> logger) : IHandleMessages<PaymentCompletedNotification>
{
	public async Task Handle(PaymentCompletedNotification message)
	{
		logger.LogInformation("Получено уведомление PaymentCompleted для заказа {OrderId}", message.OrderId);
        
		await notificationService.SendPaymentCompletedAsync(message.OrderId, message.Success, message.TransactionId);
	}
}