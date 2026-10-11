using ECommerce.Application.Features.AuditLogs.DTOs;
using ECommerce.Application.Features.AuditLogs.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/audit-logs")]
public class AdminAuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public AdminAuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [HasPermission(Permissions.AuditView)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterParams filter, CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetAuditLogsAsync(filter, cancellationToken);
        return HandleResult(result);
    }
}
