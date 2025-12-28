using Contracts.Messages.Events;
using NotificationService.Infrastructure.Services;
using Rebus.Handlers;

namespace NotificationService.Infrastructure.Consumers;

public class OrderCompletedConsumer(INotificationService notificationService, ILogger<OrderCreatedConsumer> logger) : IHandleMessages<OrderCompletedNotification>
{
	public async Task Handle(OrderCompletedNotification message)
	{
		logger.LogInformation("Получено уведомление OrderCompleted для заказа {OrderId}", message.OrderId);
        
		await notificationService.SendOrderCompletedAsync(message.OrderId, message.EstimatedDelivery);
	}
}