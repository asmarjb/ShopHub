using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Roles.DTOs;

namespace ECommerce.Application.Features.Roles.Services;

public interface IRoleService
{
    Task<Result<IReadOnlyList<RoleDto>>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> UpdatePermissionsAsync(Guid roleId, UpdateRolePermissionsDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Result<IReadOnlyList<string>> GetAllAvailablePermissions();
}
