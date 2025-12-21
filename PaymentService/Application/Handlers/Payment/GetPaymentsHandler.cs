using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Mappers;
using PaymentService.Dto.Payment;
using PaymentService.Infrastructure;
using PaymentService.Models;
using Requestum.Contract;

namespace PaymentService.Application.Handlers.Payment;

public class GetPaymentsHandler(PaymentDbContext paymentDbContext) : IAsyncQueryHandler<GetPaymentsQuery, List<GetPaymentDto>>
{
    public async Task<List<GetPaymentDto>> HandleAsync(GetPaymentsQuery query, CancellationToken cancellationToken)
    {
        var page = query.PaymentsSearchFilter.Page;
        var limit = query.PaymentsSearchFilter.Limit;
        var accountId = query.PaymentsSearchFilter.AccountId == null ? null : new AccountId(query.PaymentsSearchFilter.AccountId.Value);
        var orderId = query.PaymentsSearchFilter.OrderId == null ? null : new OrderId(query.PaymentsSearchFilter.OrderId.Value);
        var payments = await paymentDbContext.Payments
            .Where(p => accountId == null || p.AccountId == accountId)
            .Where(p => orderId == null || p.OrderId == orderId)
            .OrderByDescending(p => p.FinishedOnUtc)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return payments
            .Select(p => new GetPaymentDto(
                p.Id.Value,
                p.AccountId.Value,
                p.OrderId.Value,
                p.FinishedOnUtc,
                EnumMapper.PaymentStatusToString(p.PaymentStatus),
                p.OrderPrice))
            .ToList();
    }
}

public record GetPaymentsQuery(PaymentsSearchFilter PaymentsSearchFilter) : IQuery<List<GetPaymentDto>>;