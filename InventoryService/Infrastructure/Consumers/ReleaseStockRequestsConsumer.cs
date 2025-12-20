using Contracts.Messages.Events;
using Contracts.Messages.Requests;
using CSharpFunctionalExtensions;
using InventoryService.Application.Handlers;
using Outbox.Extensions;
using Rebus.Handlers;
using Requestum;

namespace InventoryService.Infrastructure.Consumers;

public class ReleaseStockRequestsConsumer(IRequestum requestum, InventoryDbContext inventoryDbContext,
    ILogger<ReleaseStockRequestsConsumer> logger) : IHandleMessages<ReleaseStockRequest>
{
    public async Task Handle(ReleaseStockRequest message)
    {
        var hasProcessedMessage = await inventoryDbContext.HasProcessedMessageAsync(message.OrderId, GetType().Name);
        if (hasProcessedMessage) return;
        var addGoodsCommand = new AddGoodsQuantityCommand(message.Items.Select(Utils.OrderItemToGoodItem).ToList(), StrictMode: true);
        var addGoodsResult = await requestum.ExecuteAsync<AddGoodsQuantityCommand, Result>(addGoodsCommand);
        if (addGoodsResult.IsFailure)
        {
            logger.LogError("Не удалось вернуть товары заказа {orderId} : {reason}", message.OrderId, addGoodsResult.Error);
            await Shared.Utils.TryAddOutboxMessageAsync(inventoryDbContext, logger, nameof(StockReleaseFailed),
                new StockReleaseFailed(message.OrderId));
        }
        else
        {
            logger.LogInformation("Товары заказа {orderId} успешно возвращены", message.OrderId);
            await Shared.Utils.TryAddOutboxMessageAsync(inventoryDbContext, logger,nameof(StockReleased),
                new StockReleased(message.OrderId));
        }
        await inventoryDbContext.AddProcessedMessageAsync(message.OrderId, GetType().Name);
        try
        {
            await inventoryDbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось вернуть товары заказа {orderId}" , message.OrderId);
            throw;
        }
    }
}