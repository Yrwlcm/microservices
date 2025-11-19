using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Outbox.Models;

namespace Outbox.EntityFrameworkConfigurations;

internal class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(om => om.Id);
        builder
            .Property(om => om.MessageStatus)
            .HasConversion<string>();
        builder
            .Property(om => om.PayloadType)
            .IsRequired()
            .HasMaxLength(200);
        builder
            .Property(om => om.Payload)
            .IsRequired()
            .HasMaxLength(3000);
    }
}