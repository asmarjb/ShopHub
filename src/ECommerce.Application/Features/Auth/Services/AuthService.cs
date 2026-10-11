using System.Security.Claims;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Auth.DTOs;
using ECommerce.Application.Features.Cart.Services;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ICartService _cartService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<AppRole> roleManager,
        ITokenService tokenService,
        IEmailService emailService,
        ICartService cartService,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _cartService = cartService;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return Result.Failure<AuthResponseDto>($"User with email '{dto.Email}' already exists.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return Result.Failure<AuthResponseDto>(string.Join(", ", createResult.Errors.Select(e => e.Description)));

        // Assign default Customer role
        await _userManager.AddToRoleAsync(user, "Customer");

        // Merge anonymous cart if one was created prior to registration
        if (!string.IsNullOrWhiteSpace(dto.AnonymousCartId))
        {
            await _cartService.MergeCartAsync(dto.AnonymousCartId, user.Id, cancellationToken);
        }

        // Send email confirmation link
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        _ = _emailService.SendEmailConfirmationAsync(user.Email, $"/confirm-email?email={user.Email}&token={Uri.EscapeDataString(emailToken)}", cancellationToken);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Result.Failure<AuthResponseDto>("Invalid email or password.");

        if (!user.IsActive)
            return Result.Failure<AuthResponseDto>("Your account has been deactivated. Please contact support.");

        // Check password with lockoutOnFailure: true
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
            return Result.Failure<AuthResponseDto>("Your account is temporarily locked due to multiple failed login attempts. Please try again in 5 minutes.");

        if (!signInResult.Succeeded)
            return Result.Failure<AuthResponseDto>("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Merge anonymous cart if passed
        if (!string.IsNullOrWhiteSpace(dto.AnonymousCartId))
        {
            await _cartService.MergeCartAsync(dto.AnonymousCartId, user.Id, cancellationToken);
        }

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
        if (principal == null)
            return Result.Failure<AuthResponseDto>("Invalid access token.");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? principal.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Result.Failure<AuthResponseDto>("Invalid token claims.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
            return Result.Failure<AuthResponseDto>("User not found or deactivated.");

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken && rt.UserId == userId, cancellationToken);

        if (storedToken == null)
            return Result.Failure<AuthResponseDto>("Refresh token not found.");

        if (storedToken.IsRevoked)
            return Result.Failure<AuthResponseDto>("Refresh token has been revoked.");

        if (storedToken.IsUsed)
            return Result.Failure<AuthResponseDto>("Refresh token has already been used.");

        if (storedToken.IsExpired)
            return Result.Failure<AuthResponseDto>("Refresh token has expired. Please log in again.");

        // Mark current token as used
        storedToken.IsUsed = true;
        await _context.SaveChangesAsync(cancellationToken);

        // Generate new token pair (rotation)
        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<Result> RevokeTokenAsync(RevokeTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken, cancellationToken);

        if (token == null)
            return Result.Failure(Error.NotFound("RefreshToken", dto.RefreshToken));

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId.Value && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Result.Failure("User not found.");

        var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Do not reveal that the user does not exist for security reasons
            return Result.Success();
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(resetToken)}";

        await _emailService.SendPasswordResetEmailAsync(user.Email!, resetLink, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Result.Failure("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<CurrentUserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<CurrentUserDto>(Error.Unauthorized());

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            return Result.Failure<CurrentUserDto>(Error.NotFound("User", userId.Value));

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(roles);

        return Result.Success(new CurrentUserDto(
            user.Id,
            user.Email ?? "",
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            roles.ToList(),
            permissions));
    }

    private async Task<Result<AuthResponseDto>> GenerateAuthResponseAsync(AppUser user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(roles);

        var (accessToken, expiresAt, jwtId) = await _tokenService.GenerateAccessTokenAsync(user, roles, permissions);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, jwtId, cancellationToken);

        return Result.Success(new AuthResponseDto(
            user.Id,
            user.Email ?? "",
            user.FirstName,
            user.LastName,
            roles.ToList(),
            permissions,
            accessToken,
            refreshToken.Token,
            expiresAt));
    }

    private async Task<List<string>> GetUserPermissionsAsync(IList<string> roles)
    {
        var permissions = new HashSet<string>();

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in claims.Where(c => c.Type == "permission"))
                {
                    permissions.Add(claim.Value);
                }
            }
        }

        return permissions.OrderBy(p => p).ToList();
    }
}
