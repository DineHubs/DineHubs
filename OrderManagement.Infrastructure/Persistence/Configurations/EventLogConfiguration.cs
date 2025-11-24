using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable("EventLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LogLevel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.Exception)
            .HasColumnType("text");

        builder.Property(x => x.Properties)
            .HasColumnType("jsonb");

        builder.Property(x => x.Source)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Indexes for query performance
        builder.HasIndex(x => x.LogLevel);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.LogLevel, x.CreatedAt });
    }
}

