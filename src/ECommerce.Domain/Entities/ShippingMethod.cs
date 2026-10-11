using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ShippingMethod : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string EstimatedDeliveryDays { get; set; } = string.Empty; // e.g., "2-3 business days"
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
