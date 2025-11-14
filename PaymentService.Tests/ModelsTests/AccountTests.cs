using FluentAssertions;
using PaymentService.Dto.Account;
using PaymentService.Models;

namespace PaymentService.Tests.ModelsTests;

[TestFixture]
    public class AccountTests
    {
        [Test]
        public void Create_ShouldInitializeAccountWithZeroBalance()
        {
            var dto = new CreateAccountDto(Guid.NewGuid());
            
            var account = Account.Create(dto);
            
            account.Id.Value.Should().Be(dto.AccountId);
            account.Balance.Should().Be(0);
            account.RowVersion.Should().BeNull();
        }

        [Test]
        public void AddMoney_ShouldIncreaseBalance_WhenAmountPositive()
        {
            var dto = new CreateAccountDto(Guid.NewGuid());
            var account = Account.Create(dto);

            var result = account.AddMoney(100);

            result.IsSuccess.Should().BeTrue();
            account.Balance.Should().Be(100);
        }

        [Test]
        public void AddMoney_ShouldFail_WhenAmountIsZeroOrNegative()
        {
            var dto = new CreateAccountDto(Guid.NewGuid());
            var account = Account.Create(dto);

            var zeroResult = account.AddMoney(0);
            var negativeResult = account.AddMoney(-50);

            zeroResult.IsFailure.Should().BeTrue();
            negativeResult.IsFailure.Should().BeTrue();
            account.Balance.Should().Be(0);
        }

        [Test]
        public void TakeMoney_ShouldDecreaseBalance_WhenEnoughFunds()
        {
            var dto = new CreateAccountDto(Guid.NewGuid());
            var account = Account.Create(dto);
            account.AddMoney(200);

            var result = account.TakeMoney(50);

            result.IsSuccess.Should().BeTrue();
            account.Balance.Should().Be(150);
        }

        [Test]
        public void TakeMoney_ShouldFail_WhenAmountNegative()
        {
            var dto = new CreateAccountDto(Guid.NewGuid());
            var account = Account.Create(dto);
            account.AddMoney(100);

            var result = account.TakeMoney(-10);

            result.IsFailure.Should().BeTrue();
            account.Balance.Should().Be(100);
        }

        [Test]
        public void TakeMoney_ShouldFail_WhenInsufficientFunds()
        {
            var dto = new CreateAccountDto(Guid.NewGuid());
            var account = Account.Create(dto);
            account.AddMoney(50);

            var result = account.TakeMoney(100);

            result.IsFailure.Should().BeTrue();
            account.Balance.Should().Be(50);
        }
    }