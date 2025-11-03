using System.ComponentModel.DataAnnotations;

namespace OrderService.Orders;

public sealed class OrderRequest
{
    [Required]
    public Guid OrderId { get; init; }

    [Required]
    public List<OrderLineRequest> Items { get; init; } = new();
}

public sealed class OrderLineRequest
{
    [Required]
    public string Sku { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Qty { get; init; }
}
