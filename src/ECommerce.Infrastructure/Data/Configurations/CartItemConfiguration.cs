using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.HasIndex(ci => new { ci.CartId, ci.ProductVariantId })
            .IsUnique();

        builder.HasOne(ci => ci.ProductVariant)
            .WithMany(pv => pv.CartItems)
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
