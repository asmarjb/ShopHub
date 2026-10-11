namespace ECommerce.Application.Features.Auth.DTOs;

public record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? AnonymousCartId = null);

public record LoginDto(
    string Email,
    string Password,
    string? AnonymousCartId = null);

public record AuthResponseDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    List<string> Roles,
    List<string> Permissions,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

public record RefreshTokenRequestDto(
    string AccessToken,
    string RefreshToken);

public record RevokeTokenRequestDto(
    string RefreshToken);

public record ForgotPasswordDto(
    string Email);

public record ResetPasswordDto(
    string Email,
    string Token,
    string NewPassword);

public record ConfirmEmailDto(
    string Email,
    string Token);

public record CurrentUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    List<string> Roles,
    List<string> Permissions);
