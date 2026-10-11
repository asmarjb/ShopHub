using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Subtitle)
            .HasMaxLength(250);

        builder.Property(b => b.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.TargetUrl)
            .HasMaxLength(500);
    }
}
