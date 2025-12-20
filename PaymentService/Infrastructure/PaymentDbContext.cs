using Microsoft.EntityFrameworkCore;
using Outbox;
using Outbox.Extensions;
using Outbox.Models;
using PaymentService.Models;

namespace PaymentService.Infrastructure;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options), IOutboxDbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<OutboxProcessedMessage> OutboxProcessedMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseOutbox();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}