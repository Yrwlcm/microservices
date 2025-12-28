namespace Contracts.Messages.Events;

public class OrderCompletedNotification : NotificationEvent
{
	public DateTime EstimatedDelivery { get; set; }
}