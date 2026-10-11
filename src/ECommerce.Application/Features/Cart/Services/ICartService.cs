using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Cart.DTOs;

namespace ECommerce.Application.Features.Cart.Services;

public interface ICartService
{
    Task<Result<CartDto>> GetCartAsync(string? anonymousId = null, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> AddItemAsync(AddToCartDto dto, string? anonymousId = null, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> UpdateItemQuantityAsync(Guid cartItemId, UpdateCartItemDto dto, string? anonymousId = null, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> RemoveItemAsync(Guid cartItemId, string? anonymousId = null, CancellationToken cancellationToken = default);
    Task<Result> ClearCartAsync(string? anonymousId = null, CancellationToken cancellationToken = default);
    Task<Result> MergeCartAsync(string anonymousId, Guid userId, CancellationToken cancellationToken = default);
}
