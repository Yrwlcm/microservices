using CSharpFunctionalExtensions;
using InventoryService.Dto;
using InventoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Requestum.Contract;
using ILogger = Serilog.ILogger;

namespace InventoryService.Application.Handlers;

public class ReleaseOrAddGoodsQuantityHandler(InventoryDbContext inventoryDbContext, ILogger logger) : IAsyncCommandHandler<ReleaseOrAddGoodsQuantityCommand, Result>
{
    public async Task<Result> ExecuteAsync(ReleaseOrAddGoodsQuantityCommand command, CancellationToken cancellationToken = default)
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
            var addQuantityRes = g.ReleaseOrAddGoods(skuQuantityDictionary[g.Sku], command.FromReserved);
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
            logger.Error("Произошла ошибка при сохранении изменений в обработчике {@handlerName}", nameof(ReleaseOrAddGoodsQuantityHandler));
            return Result.Failure("Ошибка сохранения данных");
        }
    }
}

public record ReleaseOrAddGoodsQuantityCommand(List<GoodQuantityDto> GoodsQuantities, bool FromReserved = false) : ICommand<Result>;