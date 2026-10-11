using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Navigation property
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
