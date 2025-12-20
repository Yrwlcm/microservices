using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PaymentService.Application.Mappers;
using PaymentService.Dto.Payment;
using PaymentService.Enums;
using PaymentService.Infrastructure;
using PaymentService.Models;
using Requestum.Contract;
using ILogger = Serilog.ILogger;

namespace PaymentService.Application.Handlers.Payment;

public class CreatePaymentHandler(PaymentDbContext paymentDbContext, ILogger logger) : IAsyncCommandHandler<CreatePaymentCommand, TransactionResult>
{
    public async Task<TransactionResult> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var accountId = new AccountId(command.AccountId);
        var orderId = new OrderId(command.OrderId);
        var account = await paymentDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        if (account == null)
        {
            var finalizeRes = await FinalizePaymentTransactionAsync(command.Transaction, accountId, orderId, null,
                PaymentStatus.MissingAccountFailure, command.Price, cancellationToken);
            return finalizeRes with { Result = Result.Failure(EnumMapper.PaymentStatusToString(PaymentStatus.MissingAccountFailure)) };
        }
        var takeMoneyRes = account.TakeMoney(command.Price);
        if (takeMoneyRes.IsFailure)
        {
            var finalizeRes = await FinalizePaymentTransactionAsync(command.Transaction, accountId, orderId, null,
                PaymentStatus.LowBalanceFailure, command.Price, cancellationToken);
            return finalizeRes with { Result = Result.Failure(takeMoneyRes.Error) };
        }
        DateTime paymentFinishedOnUtc;
        try
        {
            await paymentDbContext.SaveChangesAsync(cancellationToken);
            paymentFinishedOnUtc = DateTime.UtcNow;
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.Fatal("Произошла ошибка при списании средств со счета {@accountId} на оплату заказа {@orderId}",
                command.AccountId, command.OrderId);
            await command.Transaction.RollbackAsync(cancellationToken);
            return new TransactionResult(Result.Failure(EnumMapper.PaymentStatusToString(PaymentStatus.BalanceError)), true);
        }
        return await FinalizePaymentTransactionAsync(command.Transaction, accountId, orderId, paymentFinishedOnUtc,
            PaymentStatus.Success, command.Price, cancellationToken);
    }

    private async Task<TransactionResult> FinalizePaymentTransactionAsync(IDbContextTransaction dbContextTransaction, AccountId accountId,
        OrderId orderId, DateTime? finishedOnUtc, PaymentStatus paymentStatus, decimal orderPrice, CancellationToken cancellationToken)
    {
        var paymentCreateDto = new CreatePaymentDto(accountId, orderId, finishedOnUtc ?? DateTime.UtcNow, paymentStatus,
            orderPrice);
        var payment = Models.Payment.Create(paymentCreateDto);
        if (payment.IsFailure)
        {
            logger.Fatal("Не удалось записать информацию о попытке оплаты заказа {@orderId} клиентом {@accountId}, статус: {@paymentStatus} " +
                               "по причине ошибки создания модели Payment: {@error}", orderId.Value, accountId.Value, paymentStatus.ToString(), payment.Error);
            await dbContextTransaction.RollbackAsync(cancellationToken);
            return new TransactionResult(Result.Failure(payment.Error), true);
        }
        try
        {
            await paymentDbContext.Payments.AddAsync(payment.Value, cancellationToken);
            await paymentDbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            logger.Fatal("Не удалось записать информацию о попытке оплаты заказа {@orderId} клиентом {@accountId}, статус: {@paymentStatus} " +
                                "по причине ошибки сохранения данных", orderId.Value, accountId.Value, paymentStatus.ToString());
            await dbContextTransaction.RollbackAsync(cancellationToken);
            return new TransactionResult(Result.Failure("Ошибка сохранения данных об оплате"), true);
        }
        return new TransactionResult(Result.Success(), false);
    }
}

public record CreatePaymentCommand(IDbContextTransaction Transaction, Guid AccountId, Guid OrderId, decimal Price) : ICommand<TransactionResult>;

public record TransactionResult(Result Result, bool WasRolledBack);