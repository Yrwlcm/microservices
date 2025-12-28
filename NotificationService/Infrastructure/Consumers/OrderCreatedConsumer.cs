using Contracts.Messages.Events;
using NotificationService.Infrastructure.Services;
using Rebus.Handlers;

namespace NotificationService.Infrastructure.Consumers;

public class OrderCreatedConsumer(
	INotificationService notificationService,
	ILogger<OrderCreatedConsumer> logger)
	: IHandleMessages<OrderCreatedNotification>
{
	public async Task Handle(OrderCreatedNotification message)
	{
		logger.LogInformation("Получено уведомление OrderCreated для заказа {OrderId}", message.OrderId);
        
		await notificationService.SendOrderCreatedAsync(message.OrderId, message.Amount, message.Items);
	}
}