using System.ComponentModel.DataAnnotations;

namespace OrderService.Models.Orders;

public class OrderRequest
{
    [Required]
    public Guid OrderId { get; init; }

    [Required]
    public OrderLineRequest[] Items { get; init; } = [];
}