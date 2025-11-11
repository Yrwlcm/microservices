using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure;

namespace PaymentService;

public static class MigrationsRunner
{
    public static void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var inventoryDbContext =  scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        inventoryDbContext.Database.Migrate();
    }
}