using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public OrderStatus? OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Order Order { get; set; } = null!;
}
