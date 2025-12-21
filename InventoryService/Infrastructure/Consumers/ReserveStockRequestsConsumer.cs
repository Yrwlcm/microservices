using Contracts.Messages.Events;
using Contracts.Messages.Requests;
using CSharpFunctionalExtensions;
using InventoryService.Application.Handlers;
using InventoryService.Dto;
using Outbox.Extensions;
using Rebus.Handlers;
using Requestum;

namespace InventoryService.Infrastructure.Consumers;

public class ReserveStockRequestsConsumer(IRequestum requestum,
    InventoryDbContext inventoryDbContext,
    ILogger<ReserveStockRequestsConsumer> logger) : IHandleMessages<ReserveStockRequest>
{
    public async Task Handle(ReserveStockRequest message)
    {
        var hasProcessedMessage = await inventoryDbContext.HasProcessedMessageAsync(message.OrderId, GetType().Name);
        if (hasProcessedMessage) return;
        var reserveGoodsCommand = new ReserveGoodsCommand(message.Items.Select(Utils.OrderItemToGoodItem).ToList());
        var reserveStockResult = await requestum.ExecuteAsync<ReserveGoodsCommand, Result<decimal>>(reserveGoodsCommand);
        if (reserveStockResult.IsFailure)
        {
            logger.LogError("Не удалось зарезервировать товары для заказа {orderId} : {reason}", message.OrderId, reserveStockResult.Error);
            await Shared.Utils.TryAddOutboxMessageAsync(inventoryDbContext, logger,nameof(StockFailed), new StockFailed(message.OrderId));
        }
        else
        {
            logger.LogInformation("Товары для заказа {orderId} успешно зарезервированы", message.OrderId);
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
            throw;
        }
    }
}
