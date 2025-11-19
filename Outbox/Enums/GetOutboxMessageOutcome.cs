namespace Outbox.Enums;

public enum GetOutboxMessageOutcome
{
    NoRecords = 0,
    UnknownPayloadType = 1,
    GotNextMessage = 2
}