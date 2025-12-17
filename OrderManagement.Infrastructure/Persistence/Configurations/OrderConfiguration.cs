using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(100);
        builder.Property(o => o.PaymentTiming).IsRequired().HasConversion<int>();
        builder.Property(o => o.CancellationReason).HasMaxLength(500);
        builder.OwnsMany(o => o.Lines, navigationBuilder =>
        {
            navigationBuilder.ToTable("OrderLines");
            navigationBuilder.WithOwner().HasForeignKey("OrderId");
            navigationBuilder.Property<Guid>("Id");
            navigationBuilder.HasKey("Id");
            
            // Explicitly map all OrderLine properties
            navigationBuilder.Property(l => l.MenuItemId)
                .IsRequired()
                .HasColumnName("MenuItemId");
            
            navigationBuilder.Property(l => l.Name)
                .IsRequired()
                .HasColumnName("Name")
                .HasMaxLength(500);
            
            navigationBuilder.Property(l => l.Price)
                .IsRequired()
                .HasColumnName("Price")
                .HasColumnType("numeric(18,2)");
            
            navigationBuilder.Property(l => l.Quantity)
                .IsRequired()
                .HasColumnName("Quantity");
        });
    }
}


