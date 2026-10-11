using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty; // e.g. "SoftDelete", "Restore", "HardDelete", "RoleChange", "PermissionChange", "OrderStatusChange"
    public string EntityName { get; set; } = string.Empty; // e.g. "Product", "Category", "Role", "Order"
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; } // JSON snapshot of previous state
    public string? NewValues { get; set; } // JSON snapshot of updated state
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
