namespace Contracts.Messages.Events;

public class OrderCreatedNotification : NotificationEvent
{
	public decimal Amount { get; set; }
	public List<string> Items { get; set; } = [];
}