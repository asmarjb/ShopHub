using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Brand : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
