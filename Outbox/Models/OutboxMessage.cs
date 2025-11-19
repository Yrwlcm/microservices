using System.ComponentModel.DataAnnotations.Schema;
using Outbox.Enums;

namespace Outbox.Models;

[Table("OutboxMessages")]
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string PayloadType { get; set; }
    public string Payload { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ProcessedOnUtc  { get; set; }
    public MessageStatus MessageStatus { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? CanTryNextAttemptOnUtc { get; set; }
    public string? Error { get; set; }
}