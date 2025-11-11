using CSharpFunctionalExtensions;
using InventoryService.Dto;
using InventoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Requestum.Contract;
using ILogger = Serilog.ILogger;

namespace InventoryService.Application.Handlers;

public class AddGoodsQuantityHandler(InventoryDbContext inventoryDbContext, ILogger logger) : IAsyncCommandHandler<AddGoodsQuantityCommand, Result>
{
    public async Task<Result> ExecuteAsync(AddGoodsQuantityCommand command, CancellationToken cancellationToken = default)
    {
        Dictionary<string, int> skuQuantityDictionary = new();
        foreach (var dto in command.GoodsQuantities)
            skuQuantityDictionary[dto.Sku.Trim().ToUpperInvariant()] = dto.Quantity;
        var skuHashSet = new HashSet<string>(skuQuantityDictionary.Keys);
        var existingGoods = await inventoryDbContext.Goods
            .Where(g => skuHashSet.Contains(g.Sku))
            .ToListAsync(cancellationToken);
        foreach (var g in existingGoods)
        {
            var addQuantityRes = g.AddGoods(skuQuantityDictionary[g.Sku]);
            if (addQuantityRes.IsFailure)
                logger.Warning("Не удалось увеличить количество товара {@sku}: {@reason}", g.Sku,  addQuantityRes.Error);
            skuHashSet.Remove(g.Sku);
        }
        foreach (var sku in skuHashSet)
            logger.Warning("Товар {@sku} не был обнаружен для добавления количества", sku);
        try
        {
            await inventoryDbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.Error("Произошла ошибка при сохранении изменений в обработчике {@handlerName}", nameof(AddGoodsQuantityHandler));
            return Result.Failure("Ошибка сохранения данных");
        }
    }
}

public record AddGoodsQuantityCommand(List<GoodQuantityDto> GoodsQuantities) : ICommand<Result>;