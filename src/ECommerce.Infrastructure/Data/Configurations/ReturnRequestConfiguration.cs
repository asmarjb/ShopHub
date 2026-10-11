using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.Reason)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rr => rr.CustomerNote)
            .HasMaxLength(1000);

        builder.Property(rr => rr.AdminNote)
            .HasMaxLength(1000);

        builder.Property(rr => rr.ProcessedBy)
            .HasMaxLength(100);

        builder.HasOne(rr => rr.Order)
            .WithMany(o => o.ReturnRequests)
            .HasForeignKey(rr => rr.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rr => rr.OrderItem)
            .WithMany()
            .HasForeignKey(rr => rr.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rr => rr.User)
            .WithMany(u => u.ReturnRequests)
            .HasForeignKey(rr => rr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
