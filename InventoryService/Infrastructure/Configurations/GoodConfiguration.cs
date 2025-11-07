using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Infrastructure.Configurations;

public class GoodConfiguration : IEntityTypeConfiguration<Good>
{
    public void Configure(EntityTypeBuilder<Good> builder)
    {
        builder.HasKey(g => g.Sku);
        builder
            .Property(g => g.Sku)
            .IsRequired()
            .HasMaxLength(100);
        builder
            .Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder
            .Property(g => g.AvailableQuantity)
            .IsRequired();
        builder
            .Property(g => g.RowVersion)
            .IsRowVersion();
    }
}