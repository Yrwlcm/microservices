namespace Contracts.Messages.Events;

public class StockReservedNotification : NotificationEvent
{
	public bool Success { get; set; }
	public string? Reason { get; set; }
}