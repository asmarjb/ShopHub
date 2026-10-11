using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Users.DTOs;

namespace ECommerce.Application.Features.Users.Services;

public interface IUserService
{
    Task<Result<PagedResult<UserDto>>> GetUsersAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<UserDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> SetUserStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task<Result> AssignRolesAsync(Guid id, List<string> roles, CancellationToken cancellationToken = default);
}
