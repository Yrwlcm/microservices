using Microsoft.EntityFrameworkCore;
using PaymentService.Dto.Account;
using PaymentService.Infrastructure;
using PaymentService.Models;
using Requestum.Contract;

namespace PaymentService.Application.Handlers.Account;

public class GetAccountHandler(PaymentDbContext paymentDbContext) : IAsyncQueryHandler<GetAccountQuery, GetAccountDto?>
{
    public async Task<GetAccountDto?> HandleAsync(GetAccountQuery query, CancellationToken cancellationToken)
    {
        var accountId = new AccountId(query.AccountId);
        var account = await paymentDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        return account == null 
            ? null 
            : new GetAccountDto(account.Id.Value, account.Balance);
    }
}

public record GetAccountQuery(Guid AccountId) : IQuery<GetAccountDto?>;