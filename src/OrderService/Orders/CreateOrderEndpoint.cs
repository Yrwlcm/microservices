using System.Linq;
using Contracts.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace OrderService.Orders;

internal sealed class CreateOrderEndpoint
{
    private readonly IBus bus;
    private readonly ILogger<CreateOrderEndpoint> logger;
    private readonly OrderRequestValidator validator;

    public CreateOrderEndpoint(IBus bus, ILogger<CreateOrderEndpoint> logger, OrderRequestValidator validator)
    {
        this.bus = bus;
        this.logger = logger;
        this.validator = validator;
    }

    public async Task<IResult> HandleAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        var validationErrors = validator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return Results.BadRequest(new { errors = validationErrors });
        }

        var orderCreated = new OrderCreated(
            request.OrderId,
            request.Items.Select(i => new OrderItem(i.Sku, i.Qty)).ToList());

        logger.LogInformation("Publishing order {OrderId} with {ItemCount} item(s)", request.OrderId, orderCreated.Items.Count);

        cancellationToken.ThrowIfCancellationRequested();
        await bus.Publish(orderCreated);

        return Results.Accepted($"/order/{request.OrderId}", new { request.OrderId });
    }
}
