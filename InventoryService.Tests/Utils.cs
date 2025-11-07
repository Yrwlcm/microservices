using InventoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Tests;

public static class Utils
{
    public static InventoryDbContext MockInventoryDbContext()
    {
        var dbContextOptions = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var dbContext = new InventoryDbContext(dbContextOptions);
        return dbContext;
    }
}