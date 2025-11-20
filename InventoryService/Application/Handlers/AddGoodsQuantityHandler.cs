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
        if (existingGoods.Count != skuHashSet.Count && command.StrictMode)
            return Result.Failure("Ошибка операции добавления товаров: не все позиции существуют на складе");
        foreach (var g in existingGoods)
        {
            var addQuantityRes = g.AddGoods(skuQuantityDictionary[g.Sku]);
            if (addQuantityRes.IsFailure && !command.StrictMode)
                logger.Warning("Не удалось увеличить количество товара {@sku}: {@reason}", g.Sku,  addQuantityRes.Error);
            if (addQuantityRes.IsFailure && command.StrictMode)
                return Result.Failure($"Ошибка операции добавления товаров: не удалось увеличить количество позиции {g.Sku}");
            skuHashSet.Remove(g.Sku);
        }
        foreach (var sku in skuHashSet)
            logger.Warning("Товар {@sku} не был обнаружен для добавления количества", sku);
        try
        {
            if (!command.StrictMode)
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

public record AddGoodsQuantityCommand(List<GoodQuantityDto> GoodsQuantities, bool StrictMode = false) : ICommand<Result>;