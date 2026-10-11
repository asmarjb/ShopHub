using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Users.DTOs;
using ECommerce.Application.Features.Users.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUsersAsync(pagination, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/status")]
    [HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> SetUserStatus(Guid id, [FromBody] UpdateUserStatusDto dto, CancellationToken cancellationToken)
    {
        var result = await _userService.SetUserStatusAsync(id, dto.IsActive, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/roles")]
    [HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignUserRolesDto dto, CancellationToken cancellationToken)
    {
        var result = await _userService.AssignRolesAsync(id, dto.Roles, cancellationToken);
        return HandleResult(result);
    }
}
