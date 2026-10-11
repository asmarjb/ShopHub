using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Wishlist.DTOs;

namespace ECommerce.Application.Features.Wishlist.Services;

public interface IWishlistService
{
    Task<Result<IReadOnlyList<WishlistItemDto>>> GetUserWishlistAsync(CancellationToken cancellationToken = default);
    Task<Result<WishlistItemDto>> AddToWishlistAsync(AddToWishlistDto dto, CancellationToken cancellationToken = default);
    Task<Result> RemoveFromWishlistAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsInWishlistAsync(Guid productId, CancellationToken cancellationToken = default);
}
