using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(sh => sh.Id);

        builder.Property(sh => sh.ChangedByName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(sh => sh.Comment)
            .HasMaxLength(500);

        builder.HasIndex(sh => sh.OrderId);
    }
}
