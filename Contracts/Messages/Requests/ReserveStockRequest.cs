using Contracts.Messages.Events;

namespace Contracts.Messages.Requests;

public record StockReserveRequest(IReadOnlyList<OrderItem> OrderItems);