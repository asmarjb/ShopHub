using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        // Unique review per user, product, and verified order
        builder.HasIndex(r => new { r.UserId, r.ProductId, r.OrderId })
            .IsUnique();

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.ModeratedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
