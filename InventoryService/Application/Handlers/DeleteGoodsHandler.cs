using InventoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Requestum.Contract;

namespace InventoryService.Application.Handlers;

public class DeleteGoodsHandler(InventoryDbContext inventoryDbContext) : IAsyncCommandHandler<DeleteGoodsCommand, int>
{
    public async Task<int> ExecuteAsync(DeleteGoodsCommand command, CancellationToken cancellationToken = default)
    {
        var skuHashset = command.Sku.Select(s => s.Trim().ToUpperInvariant()).ToHashSet();
        return await inventoryDbContext.Goods
            .Where(g => skuHashset.Contains(g.Sku))
            .ExecuteDeleteAsync(cancellationToken);
    }
}

public record DeleteGoodsCommand(List<string> Sku) : ICommand<int>;