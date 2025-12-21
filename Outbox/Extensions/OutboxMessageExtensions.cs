using Outbox.Enums;
using Outbox.Models;

namespace Outbox.Extensions;

public static class OutboxMessageExtensions
{
    public static void MarkAsFailed(this OutboxMessage message, string error)
    {
        message.MessageStatus = MessageStatus.Failed;
        message.Error = error;
        message.CanTryNextAttemptOnUtc = null;
    }

    public static void MarkAsPublished(this OutboxMessage message)
    {
        message.MessageStatus = MessageStatus.Published;
        message.CanTryNextAttemptOnUtc = null;
        message.ProcessedOnUtc = DateTime.UtcNow;
        message.Error = null;
    }

    public static void TryPlanPublishingRetry(this OutboxMessage message, string error)
    {
        if (message.AttemptCount + 1 >= Const.MaxMessagePublishingAttemptsCount)
        {
            message.MarkAsFailed("Превышено число попыток публикации сообщения");
            return;
        }
        message.Error ??= error;
        message.AttemptCount += 1;
        var nextAttemptInMinutes = (int)Math.Pow(2, message.AttemptCount);
        message.CanTryNextAttemptOnUtc = DateTime.UtcNow.AddMinutes(nextAttemptInMinutes);
    }
}