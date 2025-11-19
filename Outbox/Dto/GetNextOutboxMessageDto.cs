using Outbox.Enums;
using Outbox.Models;

namespace Outbox.Dto;

public record GetNextOutboxMessageDto(Type? Type, OutboxMessage? OutboxMessage, GetOutboxMessageOutcome
    Outcome);