using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Features.AuditLogs.DTOs;

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string? UserName,
    string Action,
    string EntityName,
    string EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    DateTime CreatedAt);

public class AuditLogFilterParams : PaginationParams
{
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
