using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using PaymentService.Dto.Account;
using PaymentService.Infrastructure;
using PaymentService.Models;
using Requestum.Contract;
using ILogger = Serilog.ILogger;

namespace PaymentService.Application.Handlers.Account;

public class CreateAccountHandler(PaymentDbContext paymentDbContext, ILogger logger) : IAsyncCommandHandler<CreateAccountCommand, Result<Guid>>
{
    public async Task<Result<Guid>> ExecuteAsync(CreateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var accountId = new AccountId(command.AccountId);
        var existingAccount = await paymentDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        if (existingAccount != null)
        {
            logger.Error("Счет {@accountId} уже создан",  command.AccountId);
            return Result.Failure<Guid>("Счет пользователя уже существует");
        }
        var account = Models.Account.Create(new CreateAccountDto(command.AccountId));
        var entry = await paymentDbContext.Accounts.AddAsync(account, cancellationToken);
        await paymentDbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(entry.Entity.Id.Value);
    }
}

public record CreateAccountCommand(Guid AccountId) : ICommand<Result<Guid>>;