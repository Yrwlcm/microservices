namespace Contracts.Messages.Events;

public class OrderFailedNotification : NotificationEvent
{
	public string Reason { get; set; } = string.Empty;
	public string Step { get; set; } = string.Empty;
}