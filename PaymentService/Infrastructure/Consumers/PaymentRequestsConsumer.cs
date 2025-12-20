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
        try
        {
            var createPaymentCommand = new CreatePaymentCommand(message.AccountId, message.OrderId, message.OrderPrice);
            var createPaymentResult = await requestum.ExecuteAsync<CreatePaymentCommand, Result>(createPaymentCommand);
            if (createPaymentResult.IsFailure)
            {
                logger.LogError("Не удалось оплатить заказ {orderId}: {reason}", message.OrderId,
                    createPaymentResult.Error);
                await Shared.Utils.TryAddOutboxMessageAsync(paymentDbContext, logger, nameof(PaymentFailed),
                    new PaymentFailed(message.OrderId));
            }
            else
            {
                logger.LogInformation("Заказ {orderId} успешно оплачен", message.OrderId);
                await Shared.Utils.TryAddOutboxMessageAsync(paymentDbContext, logger, nameof(PaymentSucceeded),
                    new PaymentSucceeded(message.OrderId));
            }

            await paymentDbContext.AddProcessedMessageAsync(message.OrderId, GetType().Name);
            await paymentDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось оплатить заказ {orderId}", message.OrderId);
            await transaction.RollbackAsync();
            throw;
        }
    }
}