using System.ComponentModel.DataAnnotations.Schema;

namespace Outbox.Models;

[Table("OutboxProcessedMessages")]
public class OutboxProcessedMessage
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string ConsumerName { get; set; }
    public DateTime ProcessedOnUtc { get; set; }
}