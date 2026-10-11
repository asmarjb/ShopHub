using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ProductNameSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oi => oi.VariantNameSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oi => oi.SKUSnapshot)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oi => oi.ProductImageUrlSnapshot)
            .HasMaxLength(500);

        builder.Property(oi => oi.UnitPriceSnapshot)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.TotalPrice)
            .HasPrecision(18, 2);

        // Crucial: SetNull upon variant hard delete to maintain order invoice integrity
        builder.HasOne(oi => oi.ProductVariant)
            .WithMany(pv => pv.OrderItems)
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
