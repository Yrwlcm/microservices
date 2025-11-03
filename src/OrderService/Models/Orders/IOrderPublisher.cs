namespace OrderService.Models.Orders;

public interface IOrderPublisher
{
    Task PublishAsync(OrderRequest request, CancellationToken cancellationToken);
}
