using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.AuditLogs.DTOs;

namespace ECommerce.Application.Features.AuditLogs.Services;

public interface IAuditLogService
{
    Task<Result<PagedResult<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterParams filter, CancellationToken cancellationToken = default);
}
