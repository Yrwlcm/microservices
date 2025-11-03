using Contracts.Messages;
using Rebus.Bus;

namespace OrderService.Orders;

internal sealed class CreateOrderEndpoint(
    IBus bus,
    ILogger<CreateOrderEndpoint> logger,
    OrderRequestValidator validator)
{
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
