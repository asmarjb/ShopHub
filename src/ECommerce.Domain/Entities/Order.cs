using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Order : BaseAuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Financial breakdown
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Shipping Method & Coupon
    public Guid? ShippingMethodId { get; set; }
    public Guid? CouponId { get; set; }
    public string? CouponCodeSnapshot { get; set; }

    // Immutable Shipping Address Snapshot
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingPhoneNumber { get; set; } = string.Empty;
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;

    public string? Notes { get; set; }

    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual ShippingMethod? ShippingMethod { get; set; }
    public virtual Coupon? Coupon { get; set; }
    public virtual Payment? Payment { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
}
