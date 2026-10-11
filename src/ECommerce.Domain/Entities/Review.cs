using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Review : BaseAuditableEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; } // Enforces verified purchase link

    public int Rating { get; set; } // 1 to 5
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public string? ModeratedBy { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
}
