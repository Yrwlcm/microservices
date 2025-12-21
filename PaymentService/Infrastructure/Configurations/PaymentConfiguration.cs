using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Models;

namespace PaymentService.Infrastructure.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PaymentStatus).HasConversion<string>();
        builder.Property(p => p.Id).HasConversion(p => p.Value, p => new PaymentId(p));
        builder.Property(p => p.AccountId).HasConversion(p => p.Value, p => new AccountId(p));
        builder.Property(p => p.OrderId).HasConversion(p => p.Value, p => new OrderId(p));
    }
}