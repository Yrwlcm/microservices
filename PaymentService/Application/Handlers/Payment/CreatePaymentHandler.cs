using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PaymentService.Application.Mappers;
using PaymentService.Dto.Payment;
using PaymentService.Enums;
using PaymentService.Infrastructure;
using PaymentService.Models;
using Requestum.Contract;

namespace PaymentService.Application.Handlers.Payment;

public class CreatePaymentHandler(PaymentDbContext paymentDbContext, ILogger<CreatePaymentHandler> logger) 
    : IAsyncCommandHandler<CreatePaymentCommand, Result> 
{
    public async Task<Result> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var accountId = new AccountId(command.AccountId);
        var orderId = new OrderId(command.OrderId);
        try
        {
            var account = await paymentDbContext.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            if (account == null)
            {
                await CreateAndSavePaymentRecordAsync(accountId, orderId, null,
                    PaymentStatus.MissingAccountFailure, command.Price, cancellationToken);

                return Result.Failure(EnumMapper.PaymentStatusToString(PaymentStatus.MissingAccountFailure));
            }

            var takeMoneyRes = account.TakeMoney(command.Price);
            if (takeMoneyRes.IsFailure)
            {
                await CreateAndSavePaymentRecordAsync(accountId, orderId, null,
                    PaymentStatus.LowBalanceFailure, command.Price, cancellationToken);

                return Result.Failure(takeMoneyRes.Error);
            }
            
            await CreateAndSavePaymentRecordAsync(accountId, orderId, DateTime.UtcNow,
                PaymentStatus.Success, command.Price, cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException e)
        {
            logger.LogError(e, "Произошла непредвиденная ошибка при сохранении данных платежа");
            throw;
        }
    }
    
    private async Task CreateAndSavePaymentRecordAsync(AccountId accountId, OrderId orderId, 
        DateTime? finishedOnUtc, PaymentStatus status, decimal price, 
        CancellationToken ct)
    {
        var dto = new CreatePaymentDto(accountId, orderId, finishedOnUtc ?? DateTime.UtcNow, status, price);
        var payment = Models.Payment.Create(dto);
        if (payment.IsFailure)
        {
             throw new Exception($"Ошибка создания модели платежа: {payment.Error}");
        }
        await paymentDbContext.Payments.AddAsync(payment.Value, ct);
        await paymentDbContext.SaveChangesAsync(ct);
    }
}

public record CreatePaymentCommand(Guid AccountId, Guid OrderId, decimal Price) : ICommand<Result>;