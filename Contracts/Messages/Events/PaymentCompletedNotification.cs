namespace Contracts.Messages.Events;

public class PaymentCompletedNotification : NotificationEvent
{
	public bool Success { get; set; }
	public string TransactionId { get; set; } = string.Empty;
	public decimal Amount { get; set; }
}