using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.OwnsMany(i => i.Movements, navigationBuilder =>
        {
            navigationBuilder.ToTable("InventoryMovements");
            navigationBuilder.WithOwner().HasForeignKey("InventoryItemId");
            navigationBuilder.Property<Guid>("Id");
            navigationBuilder.HasKey("Id");
        });
    }
}


