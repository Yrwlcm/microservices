using Contracts.Messages.Events;
using Contracts.Messages.Requests;
using CSharpFunctionalExtensions;
using InventoryService.Application.Handlers;
using JetBrains.Annotations;
using Outbox.Extensions;
using Rebus.Bus;
using Rebus.Handlers;
using Requestum;

namespace InventoryService.Infrastructure.Consumers;

[UsedImplicitly]
public class ReserveStockRequestsConsumer(IRequestum requestum, IBus bus,
    InventoryDbContext inventoryDbContext,
    ILogger<ReserveStockRequestsConsumer> logger) : IHandleMessages<ReserveStockRequest>
{
    public async Task Handle(ReserveStockRequest message)
    {
        logger.LogInformation("Обработка заказа {orderId}", message.OrderId);
        var reserveGoodsCommand = new ReserveGoodsCommand(message.Items.Select(Utils.OrderItemToGoodItem).ToList());
        var reserveStockResult = await requestum.ExecuteAsync<ReserveGoodsCommand, Result<decimal>>(reserveGoodsCommand);
        if (reserveStockResult.IsFailure)
        {
            var notification = new StockReservedNotification {
                Success = false,
                Reason =  reserveStockResult.Error,
                OrderId =  message.OrderId
            };
            await bus.Publish(notification);
            logger.LogError("Не удалось зарезервировать товары для заказа {orderId} : {reason}", message.OrderId, reserveStockResult.Error);
            await Shared.Utils.TryAddOutboxMessageAsync(inventoryDbContext, logger,nameof(StockFailed), new StockFailed(message.OrderId));
        }
        else
        {
            logger.LogInformation("Товары для заказа {orderId} успешно зарезервированы", message.OrderId);
            var notification = new StockReservedNotification {
                Success = true,
                OrderId =  message.OrderId
            };
            await bus.Publish(notification);
            await Shared.Utils.TryAddOutboxMessageAsync(inventoryDbContext, logger, nameof(StockReserved),
                new StockReserved(message.OrderId, reserveStockResult.Value));
        }
        await inventoryDbContext.AddProcessedMessageAsync(message.OrderId, GetType().Name);
        try
        {
            await inventoryDbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось зарезервировать товары для заказа {orderId}" , message.OrderId);
            var notification = new StockReservedNotification {
                Success = false,
                OrderId =  message.OrderId
            };
            await bus.Publish(notification);
            throw;
        }
    }
}
