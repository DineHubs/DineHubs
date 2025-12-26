using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("Tables");

        builder.Property(t => t.TableNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.PositionX)
            .IsRequired();

        builder.Property(t => t.PositionY)
            .IsRequired();

        builder.Property(t => t.Width)
            .IsRequired()
            .HasDefaultValue(100.0);

        builder.Property(t => t.Height)
            .IsRequired()
            .HasDefaultValue(100.0);

        // Unique constraint: table number must be unique within a branch
        builder.HasIndex(t => new { t.BranchId, t.TableNumber })
            .IsUnique();

        // Index for branch-based queries
        builder.HasIndex(t => t.BranchId);

        // Index for tenant-based queries
        builder.HasIndex(t => t.TenantId);
    }
}
