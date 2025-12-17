using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class OrderExceptionConfiguration : IEntityTypeConfiguration<OrderException>
{
    public void Configure(EntityTypeBuilder<OrderException> builder)
    {
        builder.ToTable("OrderExceptions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.OrderId).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Resolution).HasMaxLength(1000);
        builder.HasIndex(e => e.OrderId);
    }
}

