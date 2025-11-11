using Microsoft.EntityFrameworkCore;
using PaymentService.Dto.Account;
using PaymentService.Infrastructure;
using Requestum.Contract;

namespace PaymentService.Application.Handlers.Account;

public class GetAccountHandler(PaymentDbContext paymentDbContext) : IAsyncQueryHandler<GetAccountQuery, GetAccountDto?>
{
    public async Task<GetAccountDto?> HandleAsync(GetAccountQuery query, CancellationToken cancellationToken)
    {
        var account = await paymentDbContext.Accounts.FirstOrDefaultAsync(a => a.Id.Value == query.AccountId, cancellationToken);
        return account == null 
            ? null 
            : new GetAccountDto(account.Id.Value, account.Balance);
    }
}

public record GetAccountQuery(Guid AccountId) : IQuery<GetAccountDto?>;