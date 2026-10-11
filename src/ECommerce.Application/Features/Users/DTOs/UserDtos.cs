namespace ECommerce.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    List<string> Roles);

public record UserDetailDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    List<string> Roles,
    int TotalOrders,
    decimal TotalSpent);

public record UpdateUserStatusDto(
    bool IsActive);

public record AssignUserRolesDto(
    List<string> Roles);
