using FluentAssertions;
using InventoryService.Dto;
using InventoryService.Models;

namespace InventoryService.Tests.Models;

public class GoodTests
{
    private static CreateGoodDto Dto(string sku = "ABC123", string name = "Тестовый товар", int quantity = 10)
        => new(sku, name, quantity);

    [Test]
    public void Create_ShouldReturnSuccess_WhenDtoIsValid()
    {
        var result = Good.Create(Dto());

        result.IsSuccess.Should().BeTrue();
        result.Value.Sku.Should().Be("ABC123");
        result.Value.Name.Should().Be("Тестовый товар");
        result.Value.AvailableQuantity.Should().Be(10);
        result.Value.ReservedQuantity.Should().Be(0);
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
    public void Create_ShouldFail_WhenQuantityIsNegative()
    {
        var result = Good.Create(Dto(quantity: -5));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недопустимое");
    }

    [Test]
    public void ReserveGoods_ShouldDecreaseAvailable_AndIncreaseReserved_WhenEnoughQuantity()
    {
        var good = Good.Create(Dto()).Value;

        var result = good.ReserveGoods(5);

        result.IsSuccess.Should().BeTrue();
        good.AvailableQuantity.Should().Be(5);
        good.ReservedQuantity.Should().Be(5);
    }

    [Test]
    public void ReserveGoods_ShouldFail_WhenNotEnoughAvailable()
    {
        var good = Good.Create(Dto(quantity: 5)).Value;

        var result = good.ReserveGoods(10);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недостаточно товара");
        good.AvailableQuantity.Should().Be(5);
        good.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public void ReserveGoods_ShouldFail_WhenQuantityIsNegative()
    {
        var good = Good.Create(Dto()).Value;

        var result = good.ReserveGoods(-1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недопустимое количество");
        good.AvailableQuantity.Should().Be(10);
        good.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public void ReleaseOrAddGoods_ShouldIncreaseAvailable_WhenAddingNewStock()
    {
        var good = Good.Create(Dto()).Value;

        var result = good.ReleaseOrAddGoods(5);

        result.IsSuccess.Should().BeTrue();
        good.AvailableQuantity.Should().Be(15);
        good.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public void ReleaseOrAddGoods_ShouldMoveFromReserved_WhenFromReservedTrue()
    {
        var good = Good.Create(Dto()).Value;
        good.ReserveGoods(4);

        var result = good.ReleaseOrAddGoods(4, fromReserved: true);

        result.IsSuccess.Should().BeTrue();
        good.AvailableQuantity.Should().Be(10);
        good.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public void ReleaseOrAddGoods_ShouldFail_WhenQuantityNegative()
    {
        var good = Good.Create(Dto()).Value;

        var result = good.ReleaseOrAddGoods(-2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недопустимое количество");
    }

    [Test]
    public void ReleaseOrAddGoods_ShouldAllowAddingFromZero()
    {
        var good = Good.Create(Dto(quantity: 0)).Value;

        var result = good.ReleaseOrAddGoods(10);

        result.IsSuccess.Should().BeTrue();
        good.AvailableQuantity.Should().Be(10);
        good.ReservedQuantity.Should().Be(0);
    }
}