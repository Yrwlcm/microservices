using CSharpFunctionalExtensions;
using InventoryService.Application.Handlers;
using InventoryService.Dto;
using Microsoft.AspNetCore.Mvc;
using Requestum;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/goods")]
public class GoodsController(IRequestum mediator) : ControllerBase
{

    /// <summary>
    /// Получить все товары
    /// </summary>
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll([FromQuery] GoodsFilter goodsFilter, CancellationToken cancellationToken)
    {
        var goods = await mediator.HandleAsync<GetGoodsQuery, List<GetGoodDto>>(new GetGoodsQuery(goodsFilter),
            cancellationToken);
        return Ok(goods);
    }

    /// <summary>
    /// Добавить новые товары в базу
    /// </summary>
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Create([FromBody] List<CreateGoodDto> createGoodDtos)
    {
        var result = await mediator.ExecuteAsync<CreateGoodCommand, Result<List<string>>>(new CreateGoodCommand(createGoodDtos));
        if (result.IsFailure) return BadRequest(result.Error);
        return Created("api/goods", result.Value);
    }
    
    /// <summary>
    /// Увеличить единицы товаров
    /// </summary>
    [HttpPut]
    [Route("quantities")]
    public async Task<IActionResult> AddGoodsQuantities([FromBody] List<GoodQuantityDto> goodQuantityDtos)
    {
        var result = await mediator.ExecuteAsync<AddGoodsQuantityCommand, Result>(
            new AddGoodsQuantityCommand(goodQuantityDtos, StrictMode: false));
        if (result.IsFailure) return BadRequest(result.Error);
        return NoContent();
    }

    /// <summary>
    /// Удалить товары по идентификаторам товарной позиции
    /// </summary>
    [HttpDelete]
    [Route("")]
    public async Task<IActionResult> Delete([FromQuery] List<string> sku)
    {
        var deletedRowsCount = await mediator.ExecuteAsync<DeleteGoodsCommand, int>(new DeleteGoodsCommand(sku));
        return Ok($"Удалено строк: {deletedRowsCount}");
    }
}