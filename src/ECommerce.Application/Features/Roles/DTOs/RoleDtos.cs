namespace ECommerce.Application.Features.Roles.DTOs;

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    List<string> Permissions);

public record CreateRoleDto(
    string Name,
    string? Description,
    List<string>? Permissions = null);

public record UpdateRolePermissionsDto(
    List<string> Permissions);
