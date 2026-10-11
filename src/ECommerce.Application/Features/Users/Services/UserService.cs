using System.Text.Json;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Users.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        UserManager<AppUser> userManager,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<UserDto>>> GetUsersAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var search = pagination.SearchTerm.Trim().ToLower();
            query = query.Where(u => u.Email!.ToLower().Contains(search)
                                  || u.FirstName.ToLower().Contains(search)
                                  || u.LastName.ToLower().Contains(search));
        }

        query = query.OrderByDescending(u => u.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            dtos.Add(new UserDto(
                user.Id,
                user.Email ?? "",
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                roles.ToList()));
        }

        return Result.Success(PagedResult<UserDto>.Create(dtos, totalCount, pagination.PageNumber, pagination.PageSize));
    }

    public async Task<Result<UserDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.Orders)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return Result.Failure<UserDetailDto>(Error.NotFound("User", id));

        var roles = await _userManager.GetRolesAsync(user);
        var totalOrders = user.Orders.Count;
        var totalSpent = user.Orders.Sum(o => o.TotalAmount);

        return Result.Success(new UserDetailDto(
            user.Id,
            user.Email ?? "",
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            roles.ToList(),
            totalOrders,
            totalSpent));
    }

    public async Task<Result> SetUserStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return Result.Failure(Error.NotFound("User", id));

        var oldStatus = user.IsActive;
        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "User",
            EntityId = user.Id.ToString(),
            Action = "SetStatus",
            OldValues = JsonSerializer.Serialize(new { IsActive = oldStatus }),
            NewValues = JsonSerializer.Serialize(new { IsActive = isActive }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignRolesAsync(Guid id, List<string> roles, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return Result.Failure(Error.NotFound("User", id));

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return Result.Failure(string.Join(", ", removeResult.Errors.Select(e => e.Description)));

        var addResult = await _userManager.AddToRolesAsync(user, roles);
        if (!addResult.Succeeded)
            return Result.Failure(string.Join(", ", addResult.Errors.Select(e => e.Description)));

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "User",
            EntityId = user.Id.ToString(),
            Action = "AssignRoles",
            OldValues = JsonSerializer.Serialize(currentRoles),
            NewValues = JsonSerializer.Serialize(roles),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
