using Contracts.Messages.Events;
using Contracts.Messages.Requests;
using CSharpFunctionalExtensions;
using Microsoft.VisualBasic.CompilerServices;
using Outbox.Extensions;
using PaymentService.Application.Handlers.Payment;
using Rebus.Handlers;
using Requestum;

namespace PaymentService.Infrastructure.Consumers;

public class PaymentRequestsConsumer(IRequestum requestum, PaymentDbContext paymentDbContext,
    ILogger<PaymentRequestsConsumer> logger) : IHandleMessages<PaymentRequest>
{
    public async Task Handle(PaymentRequest message)
    {
        var hasProcessedMessage = await paymentDbContext.HasProcessedMessageAsync(message.OrderId, GetType().Name);
        if (hasProcessedMessage) return;
        await using var transaction = await paymentDbContext.Database.BeginTransactionAsync();
        var createPaymentCommand = new CreatePaymentCommand(transaction, message.AccountId, message.OrderId, message.OrderPrice);
        var createPaymentResult = await requestum.ExecuteAsync<CreatePaymentCommand, TransactionResult>(createPaymentCommand);
        if (createPaymentResult.Result.IsFailure)
        {
            logger.LogError("Не удалось оплатить заказ {orderId}: {reason}", message.OrderId, createPaymentResult.Result.Error);
            await Shared.Utils.TryAddOutboxMessageAsync(paymentDbContext, logger, nameof(PaymentFailed),
                new PaymentFailed(message.OrderId));
        }
        else
        {
            logger.LogInformation("Заказ {orderId} успешно оплачен", message.OrderId);
            await Shared.Utils.TryAddOutboxMessageAsync(paymentDbContext, logger,nameof(PaymentSucceeded),
                new PaymentSucceeded(message.OrderId));
        }
        await paymentDbContext.AddProcessedMessageAsync(message.OrderId, GetType().Name);
        try
        {
            if (createPaymentResult.WasRolledBack)
                await paymentDbContext.SaveChangesAsync();
            else
                await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось оплатить заказ {orderId}" , message.OrderId);
            throw;
        }
    }
}