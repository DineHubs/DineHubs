using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class MenuPermissionConfiguration : IEntityTypeConfiguration<MenuPermission>
{
    public void Configure(EntityTypeBuilder<MenuPermission> builder)
    {
        builder.ToTable("MenuPermissions");

        builder.HasKey(x => new { x.MenuItemId, x.RoleName });

        builder.Property(x => x.RoleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.MenuItemId);
        builder.HasIndex(x => x.RoleName);
    }
}

