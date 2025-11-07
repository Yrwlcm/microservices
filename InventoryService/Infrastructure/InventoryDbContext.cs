using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Good> Goods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}