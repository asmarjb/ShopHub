using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.AuditLogs.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.AuditLogs.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;

    public AuditLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(a => a.Action == filter.Action);

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
            query = query.Where(a => a.EntityName == filter.EntityName);

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(a => a.CreatedAt <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(a => (a.UserName != null && a.UserName.ToLower().Contains(search))
                                  || a.EntityId.ToLower().Contains(search)
                                  || (a.OldValues != null && a.OldValues.ToLower().Contains(search))
                                  || (a.NewValues != null && a.NewValues.ToLower().Contains(search)));
        }

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.UserId,
                a.UserName,
                a.Action,
                a.EntityName,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.IpAddress,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<AuditLogDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize));
    }
}
