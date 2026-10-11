using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Application.Features.Roles.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRolesAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("permissions")]
    [HasPermission(Permissions.RoleManage)]
    public IActionResult GetAllPermissions()
    {
        var result = _roleService.GetAllAvailablePermissions();
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateRoleAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/permissions")]
    [HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] UpdateRolePermissionsDto dto, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdatePermissionsAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteRoleAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
