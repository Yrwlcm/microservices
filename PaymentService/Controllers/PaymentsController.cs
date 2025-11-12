using Microsoft.AspNetCore.Mvc;
using PaymentService.Application;
using PaymentService.Application.Handlers.Account;
using PaymentService.Application.Handlers.Payment;
using PaymentService.Dto.Account;
using PaymentService.Dto.Payment;
using Requestum;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IRequestum requestum) : ControllerBase
{
    /// <summary>
    /// Получить информацию о платежах (все или по OrderId, AccountId) (сортировка по убыванию даты)
    /// </summary>
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Get([FromQuery] PaymentsSearchFilter paymentsSearchFilter, CancellationToken cancellationToken)
    {
        var accounts = await requestum.HandleAsync<GetPaymentsQuery, List<GetPaymentDto>>(
            new GetPaymentsQuery(paymentsSearchFilter), cancellationToken);
        return Ok(accounts);
    }
}