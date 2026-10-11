using System.Security.Claims;
using System.Text.Json;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Roles.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RoleService(
        RoleManager<AppRole> roleManager,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _roleManager = roleManager;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.AsNoTracking().ToListAsync(cancellationToken);
        var dtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
            dtos.Add(new RoleDto(role.Id, role.Name ?? "", role.Description, permissions));
        }

        return Result.Success<IReadOnlyList<RoleDto>>(dtos);
    }

    public async Task<Result<RoleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return Result.Failure<RoleDto>(Error.NotFound("Role", id));

        var claims = await _roleManager.GetClaimsAsync(role);
        var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

        return Result.Success(new RoleDto(role.Id, role.Name ?? "", role.Description, permissions));
    }

    public async Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
            return Result.Failure<RoleDto>($"Role '{dto.Name}' already exists.");

        var role = new AppRole(dto.Name, dto.Description);
        var createResult = await _roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
            return Result.Failure<RoleDto>(string.Join(", ", createResult.Errors.Select(e => e.Description)));

        if (dto.Permissions != null && dto.Permissions.Any())
        {
            foreach (var permission in dto.Permissions.Distinct())
            {
                await _roleManager.AddClaimAsync(role, new Claim("permission", permission));
                _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, Permission = permission });
            }
        }

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Role",
            EntityId = role.Id.ToString(),
            Action = "CreateRole",
            NewValues = JsonSerializer.Serialize(new { role.Name, role.Description, Permissions = dto.Permissions }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(role.Id, cancellationToken);
    }

    public async Task<Result<RoleDto>> UpdatePermissionsAsync(Guid roleId, UpdateRolePermissionsDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
            return Result.Failure<RoleDto>(Error.NotFound("Role", roleId));

        if (role.Name == "SuperAdmin")
            return Result.Failure<RoleDto>("SuperAdmin permissions cannot be modified.");

        // Clear existing permission claims
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var oldPermissions = existingClaims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        foreach (var claim in existingClaims.Where(c => c.Type == "permission"))
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Clear existing role permissions table rows
        var existingEntities = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        _context.RolePermissions.RemoveRange(existingEntities);

        // Add new permissions
        foreach (var permission in dto.Permissions.Distinct())
        {
            await _roleManager.AddClaimAsync(role, new Claim("permission", permission));
            _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, Permission = permission });
        }

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Role",
            EntityId = role.Id.ToString(),
            Action = "UpdatePermissions",
            OldValues = JsonSerializer.Serialize(oldPermissions),
            NewValues = JsonSerializer.Serialize(dto.Permissions),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(role.Id, cancellationToken);
    }

    public async Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
            return Result.Failure(Error.NotFound("Role", roleId));

        if (role.Name == "SuperAdmin" || role.Name == "Manager" || role.Name == "Customer")
            return Result.Failure($"Default system role '{role.Name}' cannot be deleted.");

        var deleteResult = await _roleManager.DeleteAsync(role);
        if (!deleteResult.Succeeded)
            return Result.Failure(string.Join(", ", deleteResult.Errors.Select(e => e.Description)));

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Role",
            EntityId = role.Id.ToString(),
            Action = "DeleteRole",
            OldValues = JsonSerializer.Serialize(new { role.Name }),
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "system",
            IpAddress = _currentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public Result<IReadOnlyList<string>> GetAllAvailablePermissions()
    {
        return Result.Success<IReadOnlyList<string>>(Permissions.All);
    }
}
