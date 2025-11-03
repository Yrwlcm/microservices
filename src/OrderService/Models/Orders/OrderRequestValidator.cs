namespace OrderService.Models.Orders;

public class OrderRequestValidator
{
    public static IReadOnlyList<string> Validate(OrderRequest request)
    {
        var errors = new List<string>();

        if (request.OrderId == Guid.Empty)
        {
            errors.Add("orderId must be a non-empty GUID.");
        }

        if (request.Items.Length == 0)
        {
            errors.Add("items must contain at least one entry.");
            return errors;
        }

        for (var index = 0; index < request.Items.Length; index++)
        {
            var item = request.Items[index];

            if (string.IsNullOrWhiteSpace(item.Sku))
            {
                errors.Add($"items[{index}].sku must be provided.");
            }

            if (item.Quantity <= 0)
            {
                errors.Add($"items[{index}].quantity must be greater than zero.");
            }
        }

        return errors;
    }
}
