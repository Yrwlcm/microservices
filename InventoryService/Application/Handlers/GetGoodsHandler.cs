using InventoryService.Dto;
using InventoryService.Extensions;
using InventoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Requestum.Contract;

namespace InventoryService.Application.Handlers;

public class GetGoodsHandler(InventoryDbContext inventoryDbContext) : IAsyncQueryHandler<GetGoodsQuery, List<GetGoodDto>>
{
    public async Task<List<GetGoodDto>> HandleAsync(GetGoodsQuery query, CancellationToken cancellationToken = default)
    {
        var goods = await inventoryDbContext.Goods
            .RetrievePage(query.GoodsFilter.Page, query.GoodsFilter.Limit)
            .ToListAsync(cancellationToken);
        return goods.Select(g => new GetGoodDto(g.Sku, g.Name, g.AvailableQuantity)).ToList();
    }
}

public record GetGoodsQuery(GoodsFilter GoodsFilter) : IQuery<List<GetGoodDto>>;