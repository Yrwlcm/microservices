using Microsoft.Extensions.Logging;
using Outbox;
using Outbox.Extensions;

namespace Shared;

public static class Utils
{
    public static async Task TryAddOutboxMessageAsync<TPayload>(IOutboxDbContext outboxDbContext, ILogger logger, string payloadType,
        TPayload payload)
    {
        var outboxMessageResult = await outboxDbContext.AddOutboxMessageAsync(payloadType,
            payload);
        if (outboxMessageResult.IsFailure)
        {
            logger.LogError("Не удалось создать Outbox сообщение типа {type}: {reason}", payloadType, outboxMessageResult.Error);
            throw new Exception("Не удалось создать Outbox сообщение");
        }
    }
}