using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation property
    public virtual Product Product { get; set; } = null!;
}
