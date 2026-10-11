using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }

    // Navigation properties
    public virtual Cart Cart { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
