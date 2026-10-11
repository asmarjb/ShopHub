using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Payment : BaseAuditableEntity
{
    public Guid OrderId { get; set; }
    public string? TransactionId { get; set; }
    public PaymentProvider Provider { get; set; } = PaymentProvider.Mock;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? PaymentDetailsJson { get; set; }
    public DateTime? PaidAt { get; set; }

    // Navigation property
    public virtual Order Order { get; set; } = null!;
}
