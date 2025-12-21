using InventoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryService;

public static class MigrationsRunner
{
    public static void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var inventoryDbContext =  scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        inventoryDbContext.Database.Migrate();
    }
}