using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(sm => sm.Reason)
            .HasMaxLength(500);

        builder.HasIndex(sm => sm.ProductVariantId);

        builder.HasOne(sm => sm.ProductVariant)
            .WithMany(pv => pv.StockMovements)
            .HasForeignKey(sm => sm.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
