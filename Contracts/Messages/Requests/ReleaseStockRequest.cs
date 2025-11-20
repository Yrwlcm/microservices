using Contracts.Messages.Events;

namespace Contracts.Messages.Requests;

public record ReleaseStockRequest(Guid OrderId, IReadOnlyList<OrderItem> Items) : IRequest;