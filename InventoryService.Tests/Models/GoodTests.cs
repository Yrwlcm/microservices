using FluentAssertions;
using InventoryService.Dto;
using InventoryService.Models;

namespace InventoryService.Tests.Models;

public class GoodTests
{
    private static CreateGoodDto Dto(string sku = "ABC123", string name = "Тестовый товар", int itemPrice=3, int quantity = 10)
        => new(sku, name, itemPrice, quantity);

    [Test]
    public void Create_ShouldReturnSuccess_WhenDtoIsValid()
    {
        var result = Good.Create(Dto());

        result.IsSuccess.Should().BeTrue();
        result.Value.Sku.Should().Be("ABC123");
        result.Value.Name.Should().Be("Тестовый товар");
        result.Value.AvailableQuantity.Should().Be(10);
    }

    [Test]
    public void Create_ShouldFail_WhenSkuIsMissing()
    {
        var result = Good.Create(Dto(sku: " "));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("идентификатор");
    }

    [Test]
    public void Create_ShouldFail_WhenNameIsMissing()
    {
        var result = Good.Create(Dto(name: ""));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("название");
    }

    [Test]
    public void Create_ShouldFail_WhenItemPriceIsInvalid()
    {
        var result = Good.Create(Dto(itemPrice: -1));
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Create_ShouldFail_WhenQuantityIsNegative()
    {
        var result = Good.Create(Dto(quantity: -5));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недопустимое");
    }

    [Test]
    public void ReserveGoods_ShouldDecreaseAvailable_WhenEnoughQuantity()
    {
        var good = Good.Create(Dto()).Value;

        var result = good.ReserveGoods(5);

        result.IsSuccess.Should().BeTrue();
        good.AvailableQuantity.Should().Be(5);
    }

    [Test]
    public void ReserveGoods_ShouldFail_WhenNotEnoughAvailable()
    {
        var good = Good.Create(Dto(quantity: 5)).Value;

        var result = good.ReserveGoods(10);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недостаточно товара");
        good.AvailableQuantity.Should().Be(5);
    }

    [Test]
    public void ReserveGoods_ShouldFail_WhenQuantityIsNegative()
    {
        var good = Good.Create(Dto()).Value;

        var result = good.ReserveGoods(-1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недопустимое количество");
        good.AvailableQuantity.Should().Be(10);
    }

    [Test]
    public void AddGoods_ShouldIncreaseAvailable_WhenAddingNewStock()
    {
        var good = Good.Create(Dto()).Value;

        var result = good.AddGoods(5);

        result.IsSuccess.Should().BeTrue();
        good.AvailableQuantity.Should().Be(15);
    }
    
    [Test]
    public void AddGoods_ShouldFail_WhenQuantityNegative()
    {
        var good = Good.Create(Dto()).Value;

        var result = good.AddGoods(-2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недопустимое количество");
    }

    [Test]
    public void ReleaseOrAddGoods_ShouldAllowAddingFromZero()
    {
        var good = Good.Create(Dto(quantity: 0)).Value;

        var result = good.AddGoods(10);

        result.IsSuccess.Should().BeTrue();
        good.AvailableQuantity.Should().Be(10);
    }
}