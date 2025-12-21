using Microsoft.EntityFrameworkCore;
using PaymentService.Dto.Account;
using PaymentService.Infrastructure;
using Requestum.Contract;

namespace PaymentService.Application.Handlers.Account;

public class GetAccountsHandler(PaymentDbContext paymentDbContext) : IAsyncQueryHandler<GetAccountsQuery, List<GetAccountDto>>
{
    public async Task<List<GetAccountDto>> HandleAsync(GetAccountsQuery query, CancellationToken cancellationToken)
    {
        var accounts = await paymentDbContext.Accounts.ToListAsync(cancellationToken);
        return accounts.Select(a => new GetAccountDto(a.Id.Value, a.Balance)).ToList();
    }
}

public record GetAccountsQuery() : IQuery<List<GetAccountDto>>;