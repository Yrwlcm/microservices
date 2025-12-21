using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using Outbox;
using Outbox.Extensions;
using Outbox.Models;

namespace InventoryService.Infrastructure;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options), IOutboxDbContext
{
    public DbSet<Good> Goods { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<OutboxProcessedMessage> OutboxProcessedMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseOutbox();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}