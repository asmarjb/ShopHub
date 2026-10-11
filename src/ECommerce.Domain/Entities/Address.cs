using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Address : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }

    // Navigation property
    public virtual AppUser User { get; set; } = null!;
}
