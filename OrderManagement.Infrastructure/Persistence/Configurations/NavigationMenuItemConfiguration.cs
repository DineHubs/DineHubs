using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class NavigationMenuItemConfiguration : IEntityTypeConfiguration<NavigationMenuItem>
{
    public void Configure(EntityTypeBuilder<NavigationMenuItem> builder)
    {
        builder.ToTable("NavigationMenuItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Icon)
            .HasMaxLength(100);

        builder.Property(x => x.Route)
            .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Self-referencing relationship for parent-child
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-many relationship with permissions
        builder.HasMany(x => x.Permissions)
            .WithOne(x => x.MenuItem)
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for tenant filtering
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => new { x.TenantId, x.DisplayOrder });
    }
}

