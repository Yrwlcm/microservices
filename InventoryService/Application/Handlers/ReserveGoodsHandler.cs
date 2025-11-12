using CSharpFunctionalExtensions;
using InventoryService.Dto;
using InventoryService.Infrastructure;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using Requestum.Contract;

namespace InventoryService.Application.Handlers;

public class ReserveGoodsHandler(InventoryDbContext inventoryDbContext) : IAsyncCommandHandler<ReserveGoodsCommand, Result<decimal>>
{
    public async Task<Result<decimal>> ExecuteAsync(ReserveGoodsCommand command, CancellationToken cancellationToken = default)
    {
        Dictionary<string, int> skuQuantityDictionary = new();
        foreach (var dto in command.GoodsQuantities)
            skuQuantityDictionary[dto.Sku.Trim().ToUpperInvariant()] = dto.Quantity;
        var skuHashset = skuQuantityDictionary.Keys.ToHashSet();
        var goods = await inventoryDbContext.Goods
            .Where(g => skuHashset.Contains(g.Sku))
            .ToListAsync(cancellationToken);
        if (goods.Count != skuHashset.Count) return OutOfStockItemsFailure(skuHashset, goods);
        decimal orderTotalPrice = 0;
        foreach (var good in goods)
        {
            var reserveGoodsRes = good.ReserveGoods(skuQuantityDictionary[good.Sku]);
            if (reserveGoodsRes.IsFailure) return Result.Failure<decimal>($"{reserveGoodsRes.Error} ({good.Sku})");
            orderTotalPrice += reserveGoodsRes.Value;
        }
        try
        {
            await inventoryDbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(orderTotalPrice);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<decimal>("Произошла ошибка при сохранении информации о зарезервированных товарах");
        }
    }

    private static Result<decimal> OutOfStockItemsFailure(HashSet<string> requestedGoods, List<Good> foundGoods)
    {
        var foundGoodsHashset = foundGoods.Select(g => g.Sku).ToHashSet();
        var outOfStockItems = requestedGoods.Except(foundGoodsHashset).ToList();
        var outOfStockItemsString =  string.Join(", ", outOfStockItems);
        return Result.Failure<decimal>($"Товары {outOfStockItemsString} отсутствуют в учёте");
    }
}

public record ReserveGoodsCommand(List<GoodQuantityDto> GoodsQuantities): ICommand<Result<decimal>>;