using Contracts.Messages.Events;

namespace Contracts.Messages.Requests;

public record ReserveStockRequest(Guid OrderId, IReadOnlyList<OrderItem> Items) : IRequest;