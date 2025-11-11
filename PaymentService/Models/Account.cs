using CSharpFunctionalExtensions;
using PaymentService.Dto.Account;

namespace PaymentService.Models;

public class Account
{
    public AccountId Id { get; private set; }
    public int Balance { get; private set; }

    public byte[]? RowVersion { get; private set; }

    private Account()
    {
        
    }

    public static Account Create(CreateAccountDto createAccountDto)
    {
        return new Account()
        {
            Id = new AccountId(createAccountDto.AccountId),
            Balance = 0
        };
    }

    public Result AddMoney(int amount)
    {
        if (amount <= 0)
            return Result.Failure("Добавляемая сумма должна быть положительной");
        Balance += amount;
        return Result.Success();
    }

    public Result TakeMoney(int amount)
    {
        if (amount < 0)
            return Result.Failure("Нельзя снять с баланса отрицательную сумму");
        if (Balance - amount < 0)
            return Result.Failure("Недостаточно средств на балансе");
        Balance -= amount;
        return Result.Success();
    }
}