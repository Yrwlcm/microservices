using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Handlers.Account;
using PaymentService.Dto.Account;
using Requestum;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(IRequestum requestum) : ControllerBase
{
    /// <summary>
    /// Получить все существующие счета
    /// </summary>
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var accounts = await requestum.HandleAsync<GetAccountsQuery, List<GetAccountDto>>(
            new GetAccountsQuery(), cancellationToken);
        return Ok(accounts);
    }

    /// <summary>
    /// Получить отдельный счет
    /// </summary>
    [HttpGet]
    [Route("{accountId:guid}")]
    public async Task<IActionResult> GetAccount([FromRoute] Guid accountId, CancellationToken cancellationToken)
    {
        var account = await requestum.HandleAsync<GetAccountQuery, GetAccountDto?>(new GetAccountQuery(accountId), cancellationToken);
        return account != null
            ? Ok(account)
            : NotFound();
    }

    /// <summary>
    /// Создать новый счет
    /// </summary>
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto createAccountDto)
    {
        var res = await requestum.ExecuteAsync<CreateAccountCommand, Result<Guid>>(new CreateAccountCommand(createAccountDto.AccountId));
        return res.IsFailure
            ? BadRequest(res.Error)
            : Created("/api/accounts", res.Value);
    }

    /// <summary>
    /// Пополнить баланс счета
    /// </summary>
    [HttpPut]
    [Route("{accountId:guid}")]
    public async Task<IActionResult> TopUpAccount([FromRoute] Guid accountId, [FromBody] TopUpAccountDto topUpAccountDto)
    {
        var res = await requestum.ExecuteAsync<TopUpAccountCommand, Result>(
            new TopUpAccountCommand(accountId, topUpAccountDto.Amount));
        return res.IsFailure
            ? BadRequest(res.Error)
            : NoContent();
    }
}