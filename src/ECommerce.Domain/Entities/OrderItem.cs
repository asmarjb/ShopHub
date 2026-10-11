using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid? ProductVariantId { get; set; } // Nullable to maintain historical integrity upon variant hard-delete

    // Product snapshots (guarantees historical immutability)
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string VariantNameSnapshot { get; set; } = string.Empty;
    public string SKUSnapshot { get; set; } = string.Empty;
    public string? ProductImageUrlSnapshot { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual ProductVariant? ProductVariant { get; set; }
}
