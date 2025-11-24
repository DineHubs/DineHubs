using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class BillingHistoryConfiguration : IEntityTypeConfiguration<BillingHistory>
{
    public void Configure(EntityTypeBuilder<BillingHistory> builder)
    {
        builder.ToTable("BillingHistory");
        builder.Property(b => b.PlanCode).HasMaxLength(100);
    }
}


