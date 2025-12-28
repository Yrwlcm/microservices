namespace Contracts;

public abstract class NotificationEvent
{
	public Guid OrderId { get; set; }
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}