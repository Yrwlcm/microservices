using Contracts.Messages.Requests;
using Rebus.Bus;

namespace InventoryService;

public static class BusSubscriber
{
    public static async Task SubscribeToMessagesAsync(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var bus =  scope.ServiceProvider.GetRequiredService<IBus>();
        await bus.Subscribe<ReserveStockRequest>();
        await bus.Subscribe<ReleaseStockRequest>();
    }
}