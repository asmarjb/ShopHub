using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductTag : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid TagId { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
