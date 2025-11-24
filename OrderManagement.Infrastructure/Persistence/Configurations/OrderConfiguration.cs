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
        builder.OwnsMany(o => o.Lines, navigationBuilder =>
        {
            navigationBuilder.ToTable("OrderLines");
            navigationBuilder.WithOwner().HasForeignKey("OrderId");
            navigationBuilder.Property<Guid>("Id");
            navigationBuilder.HasKey("Id");
        });
    }
}


