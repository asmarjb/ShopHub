using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Coupon : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponType CouponType { get; set; } = CouponType.Percentage;
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? UsageLimit { get; set; }
    public int TimesUsed { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsExpired => ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate.Value;
    public bool HasReachedLimit => UsageLimit.HasValue && TimesUsed >= UsageLimit.Value;
    public bool IsValid => IsActive && !IsExpired && !HasReachedLimit && (!StartDate.HasValue || DateTime.UtcNow >= StartDate.Value);

    // Navigation property
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
