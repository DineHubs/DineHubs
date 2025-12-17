using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class OrderMetricsConfiguration : IEntityTypeConfiguration<OrderMetrics>
{
    public void Configure(EntityTypeBuilder<OrderMetrics> builder)
    {
        builder.ToTable("OrderMetrics");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.OrderId).IsRequired();
        builder.Property(m => m.PrepTime).HasConversion(
            v => v.HasValue ? (long?)v.Value.TotalSeconds : null,
            v => v.HasValue ? TimeSpan.FromSeconds(v.Value) : null);
        builder.Property(m => m.TableTurnTime).HasConversion(
            v => v.HasValue ? (long?)v.Value.TotalSeconds : null,
            v => v.HasValue ? TimeSpan.FromSeconds(v.Value) : null);
        builder.HasIndex(m => m.OrderId);
    }
}

