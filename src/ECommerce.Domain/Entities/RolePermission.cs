using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public string Permission { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual AppRole Role { get; set; } = null!;
}
