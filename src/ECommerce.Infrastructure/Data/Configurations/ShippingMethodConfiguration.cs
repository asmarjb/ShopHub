using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sm => sm.Description)
            .HasMaxLength(500);

        builder.Property(sm => sm.Price)
            .HasPrecision(18, 2);

        builder.Property(sm => sm.EstimatedDeliveryDays)
            .IsRequired()
            .HasMaxLength(100);
    }
}
