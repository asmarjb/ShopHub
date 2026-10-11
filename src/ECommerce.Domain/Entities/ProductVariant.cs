using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductVariant : BaseAuditableEntity
{
    public Guid ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // e.g. "Size: L, Color: Blue"
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public string? ImageUrl { get; set; }
    public string? Barcode { get; set; }

    public bool IsInStock => StockQuantity > 0;
    public bool IsLowStock => StockQuantity > 0 && StockQuantity <= LowStockThreshold;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
