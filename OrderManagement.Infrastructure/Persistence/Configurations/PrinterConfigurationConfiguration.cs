using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class PrinterConfigurationConfiguration : IEntityTypeConfiguration<PrinterConfiguration>
{
    public void Configure(EntityTypeBuilder<PrinterConfiguration> builder)
    {
        builder.ToTable("PrinterConfigurations");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PrinterName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Type)
            .IsRequired();

        builder.Property(p => p.ConnectionType)
            .IsRequired();

        builder.Property(p => p.PaperWidth)
            .IsRequired()
            .HasDefaultValue(80);

        builder.Property(p => p.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(p => new { p.TenantId, p.BranchId, p.Name })
            .IsUnique();

        builder.HasIndex(p => new { p.TenantId, p.BranchId, p.Type, p.IsDefault })
            .HasFilter("\"IsDefault\" = true");
    }
}

