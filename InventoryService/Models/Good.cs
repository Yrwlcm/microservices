using CSharpFunctionalExtensions;
using InventoryService.Dto;

namespace InventoryService.Models;

public class Good
{
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public int AvailableQuantity { get; private set; }
    public int ItemPrice { get; private set; }
    public byte[]? RowVersion { get; private set; }

    private Good()
    {
        
    }

    public static Result<Good> Create(CreateGoodDto createGoodDto)
    {
        if (string.IsNullOrWhiteSpace(createGoodDto.Sku))
            return Result.Failure<Good>("Не заполнен идентификатор товарной позиции");
        if (string.IsNullOrWhiteSpace(createGoodDto.Name))
            return Result.Failure<Good>("Не заполнено отображаемое название товарной позиции");
        if (createGoodDto.ItemPrice <= 0)
            return Result.Failure<Good>("Цена единицы товара должна быть больше 0");
        if (createGoodDto.Quantity is < 0)
            return Result.Failure<Good>("Недопустимое количество товара");
        var item = new Good()
        {
            Sku = createGoodDto.Sku,
            Name = createGoodDto.Name,
            ItemPrice = createGoodDto.ItemPrice,
            AvailableQuantity = createGoodDto.Quantity ?? 0,
        };
        return Result.Success(item);
    }

    public Result<int> ReserveGoods(int quantity)
    {
        if (quantity < 0) return Result.Failure<int>("Недопустимое количество товара");
        if (AvailableQuantity < quantity) return Result.Failure<int>($"Недостаточно товара {Name} для резервирования");
        AvailableQuantity -= quantity;
        return Result.Success(quantity * ItemPrice);
    }

    public Result AddGoods(int quantity)
    {
        if (quantity < 0) return Result.Failure("Недопустимое количество товара");
        AvailableQuantity += quantity;
        return Result.Success();
    }
}