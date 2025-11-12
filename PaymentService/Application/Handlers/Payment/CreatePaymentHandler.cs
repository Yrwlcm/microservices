using System.Data;
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

public class CreatePaymentHandler(PaymentDbContext paymentDbContext, ILogger logger) : IAsyncCommandHandler<CreatePaymentCommand, Result>
{
    public async Task<Result> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await paymentDbContext.Database.BeginTransactionAsync(cancellationToken);
        var accountId = new AccountId(command.AccountId);
        var orderId = new OrderId(command.OrderId);
        var account = await paymentDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        if (account == null)
        {
            await FinalizePaymentTransactionAsync(transaction, accountId, orderId, null,
                PaymentStatus.MissingAccountFailure, command.Price, cancellationToken);
            return Result.Failure(EnumMapper.PaymentStatusToString(PaymentStatus.MissingAccountFailure));
        }
        var takeMoneyRes = account.TakeMoney(command.Price);
        if (takeMoneyRes.IsFailure)
        {
            await FinalizePaymentTransactionAsync(transaction, accountId, orderId, null,
                PaymentStatus.LowBalanceFailure, command.Price, cancellationToken);
            return Result.Failure(takeMoneyRes.Error);
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
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure(EnumMapper.PaymentStatusToString(PaymentStatus.BalanceError));
        }
        return await FinalizePaymentTransactionAsync(transaction, accountId, orderId, paymentFinishedOnUtc,
            PaymentStatus.Success, command.Price, cancellationToken);
    }

    private async Task<Result> FinalizePaymentTransactionAsync(IDbContextTransaction dbContextTransaction, AccountId accountId,
        OrderId orderId, DateTime? finishedOnUtc, PaymentStatus paymentStatus, int orderPrice, CancellationToken cancellationToken)
    {
        var paymentCreateDto = new CreatePaymentDto(accountId, orderId, finishedOnUtc ?? DateTime.UtcNow, paymentStatus,
            orderPrice);
        var payment = Models.Payment.Create(paymentCreateDto);
        if (payment.IsFailure)
        {
            logger.Fatal("Не удалось записать информацию о попытке оплаты заказа {@orderId} клиентом {@accountId}, статус: {@paymentStatus} " +
                               "по причине ошибки создания модели Payment: {@error}", orderId.Value, accountId.Value, paymentStatus.ToString(), payment.Error);
            await dbContextTransaction.RollbackAsync(cancellationToken);
            return Result.Failure(payment.Error);
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
            return Result.Failure("Ошибка сохранения данных об оплате");
        }
        await dbContextTransaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public record CreatePaymentCommand(Guid AccountId, Guid OrderId, int Price) : ICommand<Result>;