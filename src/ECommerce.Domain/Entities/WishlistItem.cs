using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
