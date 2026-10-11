using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(pv => pv.SKU)
            .IsUnique();

        builder.Property(pv => pv.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pv => pv.Color)
            .HasMaxLength(50);

        builder.Property(pv => pv.Size)
            .HasMaxLength(50);

        builder.Property(pv => pv.Price)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.ImageUrl)
            .HasMaxLength(500);

        builder.Property(pv => pv.Barcode)
            .HasMaxLength(100);
    }
}
