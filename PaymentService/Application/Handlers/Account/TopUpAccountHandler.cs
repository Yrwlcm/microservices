using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure;
using PaymentService.Models;
using Requestum.Contract;
using ILogger = Serilog.ILogger;

namespace PaymentService.Application.Handlers.Account;

public class TopUpAccountHandler(PaymentDbContext paymentDbContext, ILogger logger) : IAsyncCommandHandler<TopUpAccountCommand, Result>
{
    public async Task<Result> ExecuteAsync(TopUpAccountCommand command, CancellationToken cancellationToken = default)
    {
        var accountId = new AccountId(command.AccountId);
        var account = await paymentDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        if (account == null)
        {
            logger.Error("Невозможно пополнить счет {@accountId}: счет отсутствует", command.AccountId);
            return Result.Failure("Несуществующий счет");
        }
        var topUpBalanceRes = account.AddMoney(command.Amount);
        if (topUpBalanceRes.IsFailure)
        {
            logger.Error("Произошла ошибка при пополнении счета {@accountId}: {@error}",  command.AccountId, topUpBalanceRes.Error);
            return Result.Failure(topUpBalanceRes.Error);
        }
        try
        {
            await paymentDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.Error("Произошла ошибка сохранения баланса счета {@accountId}. Повторите попытку позже", command.AccountId);
            throw;
        }
        return Result.Success();
    }
}

public record TopUpAccountCommand(Guid AccountId, int Amount) : ICommand<Result>;