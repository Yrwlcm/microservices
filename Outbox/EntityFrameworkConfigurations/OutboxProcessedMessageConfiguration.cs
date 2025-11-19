using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Outbox.Models;

namespace Outbox.EntityFrameworkConfigurations;

internal class OutboxProcessedMessageConfiguration : IEntityTypeConfiguration<OutboxProcessedMessage>
{
    public void Configure(EntityTypeBuilder<OutboxProcessedMessage> builder)
    {
        builder.HasKey(opm => opm.Id);
        builder
            .Property(opm => opm.ConsumerName)
            .IsRequired()
            .HasMaxLength(200);
    }
}