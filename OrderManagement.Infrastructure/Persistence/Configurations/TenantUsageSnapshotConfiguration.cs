using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class TenantUsageSnapshotConfiguration : IEntityTypeConfiguration<TenantUsageSnapshot>
{
    public void Configure(EntityTypeBuilder<TenantUsageSnapshot> builder)
    {
        builder.ToTable("UsageSnapshots");
    }
}


