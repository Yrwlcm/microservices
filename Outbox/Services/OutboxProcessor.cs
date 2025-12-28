using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outbox.Enums;
using Outbox.Extensions;
using Rebus.Bus;

namespace Outbox.Services;

public class OutboxProcessor<TDbContext>(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
where TDbContext : DbContext, IOutboxDbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            var bus = scope.ServiceProvider.GetRequiredService<IBus>();
            var nextOutboxMessageResult = await dbContext.GetNextOutboxMessageAsync();
            if (nextOutboxMessageResult.Outcome == GetOutboxMessageOutcome.NoRecords)
            {
                logger.LogInformation("Таблица Outbox не содержит новых сообщений");
                await Task.Delay(2000,  stoppingToken);
                continue;
            }

            var message = nextOutboxMessageResult.OutboxMessage!;
            if (nextOutboxMessageResult.Outcome == GetOutboxMessageOutcome.UnknownPayloadType)
            {
                logger.LogError("Сообщение {outboxGuid} в таблице Outbox не может быть обработано: неизвестный тип сообщения", message.Id);
                await UpdateOutboxMessageAsync(dbContext, message.Id, () => message.MarkAsFailed("Неизвестный тип сообщения"), stoppingToken);
                await Task.Yield();
                continue;
            }

            object? deserializedPayload;
            try
            {
                deserializedPayload = JsonSerializer.Deserialize(message.Payload, nextOutboxMessageResult.Type!, JsonOptions.JsonSerializerOptions);
                if (deserializedPayload is null)
                {
                    logger.LogError("Сообщение {outboxGuid} в таблице Outbox не может быть обработано: пустой payload", message.Id);
                    await UpdateOutboxMessageAsync(dbContext, message.Id, () => message.MarkAsFailed("Пустой payload"), stoppingToken);
                    await Task.Yield();
                    continue;
                }
            }
            catch (JsonException e)
            {
                logger.LogError(e, "Сообщение {outboxGuid} в таблице Outbox не может быть обработано: не удалось десериализовать данные",
                    message.Id);
                await UpdateOutboxMessageAsync(dbContext, message.Id,
                    () => message.MarkAsFailed("Ошибка десериализации payload"), stoppingToken);
                await Task.Yield();
                continue;
            }

            try
            {
                await bus.Publish(deserializedPayload);
                logger.LogInformation("Сообщение {outboxGuid} в таблице Outbox успешно опубликовано", message.Id);
                await UpdateOutboxMessageAsync(dbContext, message.Id, () => message.MarkAsPublished(), stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Сообщение {outboxGuid} из таблицы Outbox не удалось отправить. " +
                                   "Номер попытки: {attempt}", message.Id, message.AttemptCount + 1);
                await UpdateOutboxMessageAsync(dbContext, message.Id,
                    () => message.TryPlanPublishingRetry("Не удалось доставить сообщение в брокер"), stoppingToken);
            }
        }
        logger.LogInformation("Обработчик таблицы Outbox завершает работу...");
    }
    
    private async Task UpdateOutboxMessageAsync(TDbContext dbContext, Guid messageId, Action updateMessageFunc,
        CancellationToken stoppingToken)
    {
        try
        {
            updateMessageFunc();
            await dbContext.SaveChangesAsync(stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось выполнить обновление данных сообщения Outbox {outboxGuid}", messageId);
        }
    }
}