using FluentAssertions;
using PaymentService.Dto.Payment;
using PaymentService.Enums;
using PaymentService.Models;

namespace PaymentService.Tests.ModelsTests;

[TestFixture]
public class PaymentTests
{
    [Test]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        // arrange
        var dto = new CreatePaymentDto(
            new AccountId(Guid.NewGuid()),
            new OrderId(Guid.NewGuid()),
            DateTime.UtcNow.AddSeconds(-5),
            PaymentStatus.Success,
            1200m);

        // act
        var result = Payment.Create(dto);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccountId.Should().Be(dto.AccountId);
        result.Value.OrderId.Should().Be(dto.OrderId);
        result.Value.FinishedOnUtc.Should().BeCloseTo(dto.FinishedOnUtc, TimeSpan.FromSeconds(1));
        result.Value.PaymentStatus.Should().Be(dto.PaymentStatus);
        result.Value.OrderPrice.Should().Be(dto.OrderPrice);
    }

    [Test]
    public void Create_ShouldFail_WhenFinishedOnUtcInFuture()
    {
        // arrange
        var dto = new CreatePaymentDto(
            new AccountId(Guid.NewGuid()),
            new OrderId(Guid.NewGuid()),
            DateTime.UtcNow.AddMinutes(5),
            PaymentStatus.Success,
            500m);

        // act
        var result = Payment.Create(dto);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("в будущем");
    }

    [Test]
    public void Create_ShouldFail_WhenOrderPriceNegative()
    {
        // arrange
        var dto = new CreatePaymentDto(
            new AccountId(Guid.NewGuid()),
            new OrderId(Guid.NewGuid()),
            DateTime.UtcNow,
            PaymentStatus.Success,
            -10m);

        // act
        var result = Payment.Create(dto);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Отрицательная");
    }
}