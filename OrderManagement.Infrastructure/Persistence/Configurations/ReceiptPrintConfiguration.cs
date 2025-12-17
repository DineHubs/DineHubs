using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class ReceiptPrintConfiguration : IEntityTypeConfiguration<ReceiptPrint>
{
    public void Configure(EntityTypeBuilder<ReceiptPrint> builder)
    {
        builder.ToTable("ReceiptPrints");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.OrderId).IsRequired();
        builder.Property(r => r.PaymentId).IsRequired();
        builder.Property(r => r.ReceiptUrl).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Reason).IsRequired().HasMaxLength(500);
        builder.Property(r => r.PrintedAt).IsRequired();
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.PaymentId);
    }
}

