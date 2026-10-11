using ECommerce.Application.Features.Statistics.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/statistics")]
public class AdminStatisticsController : BaseApiController
{
    private readonly IStatisticsService _statisticsService;

    public AdminStatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("dashboard")]
    [HasPermission(Permissions.StatisticsView)]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var result = await _statisticsService.GetDashboardStatsAsync(cancellationToken);
        return HandleResult(result);
    }
}
