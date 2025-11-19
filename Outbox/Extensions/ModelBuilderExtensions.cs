using Microsoft.EntityFrameworkCore;
using Outbox.Models;

namespace Outbox.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxMessage).Assembly);
    }
}