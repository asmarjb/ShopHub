using Microsoft.AspNetCore.Identity;

namespace ECommerce.Domain.Entities;

public class AppRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public AppRole() : base()
    {
    }

    public AppRole(string roleName, string? description = null) : base(roleName)
    {
        Description = description;
    }
}
