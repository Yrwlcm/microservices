namespace OrderService.Orders;

internal sealed class OrderRequestValidator
{
    public IReadOnlyList<string> Validate(OrderRequest request)
    {
        var errors = new List<string>();

        if (request.OrderId == Guid.Empty)
        {
            errors.Add("orderId must be a non-empty GUID.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            errors.Add("items must contain at least one entry.");
            return errors;
        }

        for (var index = 0; index < request.Items.Count; index++)
        {
            var item = request.Items[index];

            if (string.IsNullOrWhiteSpace(item.Sku))
            {
                errors.Add($"items[{index}].sku must be provided.");
            }

            if (item.Qty <= 0)
            {
                errors.Add($"items[{index}].qty must be greater than zero.");
            }
        }

        return errors;
    }
}
