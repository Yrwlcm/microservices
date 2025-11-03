using Microsoft.AspNetCore.Mvc;
using OrderService.Models.Orders;

namespace OrderService.Controllers;

[ApiController]
[Route("order")]
public class OrdersController(OrderRequestValidator validator, IOrderPublisher publisher) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] OrderRequest request, CancellationToken cancellationToken)
    {
        var validationErrors = OrderRequestValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return BadRequest(new { errors = validationErrors });
        }

        await publisher.PublishAsync(request, cancellationToken);
        return Accepted($"/order/{request.OrderId}", new { request.OrderId });
    }
}
