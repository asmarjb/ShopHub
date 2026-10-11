using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.UserName)
            .HasMaxLength(150);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.IpAddress)
            .HasMaxLength(50);

        builder.HasIndex(al => al.EntityName);
        builder.HasIndex(al => al.EntityId);
        builder.HasIndex(al => al.CreatedAt);
    }
}
