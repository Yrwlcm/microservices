using System.ComponentModel.DataAnnotations;

namespace OrderService.Models.Orders;

public class OrderLineRequest
{
    [Required]
    public string Sku { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}