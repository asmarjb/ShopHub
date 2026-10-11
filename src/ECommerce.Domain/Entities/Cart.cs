using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Cart : BaseAuditableEntity
{
    public Guid? UserId { get; set; }
    public string? AnonymousId { get; set; } // For guest shopping cart tracking

    // Navigation properties
    public virtual AppUser? User { get; set; }
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
