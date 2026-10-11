using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Product : BaseAuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual Brand? Brand { get; set; }
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
