using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class StockMovement : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public StockMovementType MovementType { get; set; } = StockMovementType.In;
    public int Quantity { get; set; } // positive (addition) or negative (deduction)
    public int RemainingStock { get; set; } // Snapshot of stock quantity after movement
    public string? ReferenceNumber { get; set; } // e.g., OrderNumber, Restock PO
    public string? Reason { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
