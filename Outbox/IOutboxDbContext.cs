using Microsoft.EntityFrameworkCore;
using Outbox.Models;

namespace Outbox;

public interface IOutboxDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<OutboxProcessedMessage> OutboxProcessedMessages { get; set; }
}