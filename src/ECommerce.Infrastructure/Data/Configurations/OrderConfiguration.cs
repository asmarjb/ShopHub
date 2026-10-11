using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingCost)
            .HasPrecision(18, 2);

        builder.Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.CouponCodeSnapshot)
            .HasMaxLength(50);

        builder.Property(o => o.ShippingFullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(o => o.ShippingPhoneNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(o => o.ShippingAddressLine1)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(o => o.ShippingAddressLine2)
            .HasMaxLength(250);

        builder.Property(o => o.ShippingCity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ShippingState)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ShippingPostalCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(o => o.ShippingCountry)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ShippingMethod)
            .WithMany(sm => sm.Orders)
            .HasForeignKey(o => o.ShippingMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Coupon)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(sh => sh.Order)
            .HasForeignKey(sh => sh.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
