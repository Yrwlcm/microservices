using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Outbox.Dto;
using Outbox.Enums;
using Outbox.Models;
using Outbox.Utils;

namespace Outbox.Extensions;

public static class OutboxDbContextExtensions
{
    public static async Task<Result> AddOutboxMessageAsync<TPayload>(this IOutboxDbContext outboxDbContext,
        string payloadType, TPayload payload)
    {
        try
        {
            var payloadSerialized = JsonSerializer.Serialize(payload, JsonOptions.JsonSerializerOptions);
            var outboxMessage = new OutboxMessage()
            {
                PayloadType = payloadType,
                Payload = payloadSerialized,
                MessageStatus = MessageStatus.Pending,
                AttemptCount = 0,
                CanTryNextAttemptOnUtc = DateTime.UtcNow,
                CreatedOnUtc = DateTime.UtcNow
            };
            await outboxDbContext.OutboxMessages.AddAsync(outboxMessage);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public static async Task<GetNextOutboxMessageDto> GetNextOutboxMessageAsync(this IOutboxDbContext outboxDbContext)
    {
        var nextOutboxMessage = await outboxDbContext.OutboxMessages
            .OrderBy(om => om.CreatedOnUtc)
            .Where(om => om.MessageStatus == MessageStatus.Pending && om.CanTryNextAttemptOnUtc <= DateTime.UtcNow
                                                             && om.AttemptCount < Const.MaxMessagePublishingAttemptsCount)
            .FirstOrDefaultAsync();
        if (nextOutboxMessage is null)
            return new GetNextOutboxMessageDto(null, null, GetOutboxMessageOutcome.NoRecords);
        var messageType = MessageTypeRegistry.GetContractOrDefault(nextOutboxMessage.PayloadType);
        return messageType is null 
            ? new GetNextOutboxMessageDto(null, nextOutboxMessage, GetOutboxMessageOutcome.UnknownPayloadType)
            : new GetNextOutboxMessageDto(messageType, nextOutboxMessage, GetOutboxMessageOutcome.GotNextMessage);
    }
    
    public static async Task<bool> HasProcessedMessageAsync(this IOutboxDbContext outboxDbContext, Guid messageId,
        string consumerName)
    {
        return await outboxDbContext.OutboxProcessedMessages.AnyAsync(opm => opm.MessageId == messageId &&
            opm.ConsumerName == consumerName);
    }

    public static async Task AddProcessedMessageAsync(this IOutboxDbContext outboxDbContext, Guid messageId,
        string consumerName)
    {
        var processedMessageModel = new OutboxProcessedMessage()
        {
            MessageId = messageId,
            ConsumerName = consumerName,
            ProcessedOnUtc = DateTime.UtcNow
        };
        await outboxDbContext.OutboxProcessedMessages.AddAsync(processedMessageModel);
    }
}