using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class ReturnRequest : BaseAuditableEntity
{
    public Guid OrderId { get; set; }
    public Guid OrderItemId { get; set; }
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? CustomerNote { get; set; }
    public string? AdminNote { get; set; }
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual OrderItem OrderItem { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}
