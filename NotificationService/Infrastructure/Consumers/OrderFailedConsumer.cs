using Contracts.Messages.Events;
using NotificationService.Infrastructure.Services;
using Rebus.Handlers;

namespace NotificationService.Infrastructure.Consumers;

public class OrderFailedConsumer(INotificationService notificationService, ILogger<OrderFailedConsumer> logger) : IHandleMessages<OrderFailedNotification>
{
	public async Task Handle(OrderFailedNotification message)
	{
		logger.LogInformation("Получено уведомление OrderFailed для заказа {OrderId}", message.OrderId);
        
		await notificationService.SendOrderFailedAsync(message.OrderId, message.Reason, message.Step);
	}
}