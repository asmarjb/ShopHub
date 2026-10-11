using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Statistics.DTOs;

namespace ECommerce.Application.Features.Statistics.Services;

public interface IStatisticsService
{
    Task<Result<AdminDashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}
