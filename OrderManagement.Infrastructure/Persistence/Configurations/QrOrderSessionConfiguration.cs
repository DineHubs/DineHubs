using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public class QrOrderSessionConfiguration : IEntityTypeConfiguration<QrOrderSession>
{
    public void Configure(EntityTypeBuilder<QrOrderSession> builder)
    {
        builder.ToTable("QrSessions");
        builder.Property(s => s.SessionCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.SessionCode).IsUnique();
    }
}


