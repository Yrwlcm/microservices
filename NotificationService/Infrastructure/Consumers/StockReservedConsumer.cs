using Contracts.Messages.Events;
using NotificationService.Infrastructure.Services;
using Rebus.Handlers;

namespace NotificationService.Infrastructure.Consumers;

public class StockReservedConsumer(INotificationService notificationService, ILogger<StockReservedConsumer> logger) : IHandleMessages<StockReservedNotification>
{
	public async Task Handle(StockReservedNotification message)
	{
		logger.LogInformation("Получено уведомление StockReserved для заказа {OrderId}", message.OrderId);
        
		await notificationService.SendStockReservedAsync(message.OrderId, message.Success, message.Reason);
	}
}