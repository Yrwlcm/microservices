using CSharpFunctionalExtensions;
using InventoryService.Dto;
using InventoryService.Infrastructure;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using Requestum.Contract;
using ILogger = Serilog.ILogger;

namespace InventoryService.Application.Handlers;

public class CreateGoodsHandler(InventoryDbContext inventoryDbContext, ILogger logger) : IAsyncCommandHandler<CreateGoodCommand, Result<List<string>>>
{
    public async Task<Result<List<string>>> ExecuteAsync(CreateGoodCommand command, CancellationToken cancellationToken = default)
    {
        var skuDtoDictionary = new Dictionary<string, CreateGoodDto>();
        var skuHashSet = command.CreateGoodsDto
            .Select(g =>
            {
                var correctedSku = g.Sku.Trim().ToUpperInvariant();
                skuDtoDictionary[correctedSku] = g;
                return correctedSku;
            })
            .ToHashSet();
        var existingGoods = await inventoryDbContext.Goods
            .Where(g => skuHashSet.Contains(g.Sku))
            .Select(g => g.Sku)
            .ToListAsync(cancellationToken);
        foreach (var sku in existingGoods)
        {
            skuDtoDictionary.Remove(sku);
            logger.Information("Товар с идентификатором {@sku} уже создан и не будет добавлен", sku);
        }

        List<Good> models = [];
        foreach (var pair in skuDtoDictionary)
        {
            var updatedDto = pair.Value with { Sku = pair.Key };
            var model = Good.Create(updatedDto);
            if (model.IsSuccess)
            {
                models.Add(model.Value);
                continue;
            }
            logger.Warning("Товар с идентификатором {@sku} не может быть создан: {@error}", pair.Key, model.Error);
        }
        await inventoryDbContext.Goods.AddRangeAsync(models, cancellationToken);
        await inventoryDbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(models.Select(m => m.Sku).ToList());
    }
}

public record CreateGoodCommand(List<CreateGoodDto> CreateGoodsDto) : ICommand<Result<List<string>>>;