using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Auth.DTOs;

namespace ECommerce.Application.Features.Auth.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);
    Task<Result> RevokeTokenAsync(RevokeTokenRequestDto dto, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
    Task<Result> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken cancellationToken = default);
    Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result<CurrentUserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
