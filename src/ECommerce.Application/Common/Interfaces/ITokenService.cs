using System.Security.Claims;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface ITokenService
{
    Task<(string AccessToken, DateTime ExpiresAt, string JwtId)> GenerateAccessTokenAsync(AppUser user, IList<string> roles, IList<string> permissions);
    Task<RefreshToken> GenerateRefreshTokenAsync(AppUser user, string jwtId, CancellationToken cancellationToken = default);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
