using ECommerce.Application.Features.Wishlist.DTOs;
using ECommerce.Application.Features.Wishlist.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
[Route("api/wishlist")]
public class WishlistController : BaseApiController
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken)
    {
        var result = await _wishlistService.GetUserWishlistAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> AddToWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _wishlistService.AddToWishlistAsync(new AddToWishlistDto(productId), cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _wishlistService.RemoveFromWishlistAsync(productId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("check/{productId:guid}")]
    public async Task<IActionResult> IsInWishlist(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _wishlistService.IsInWishlistAsync(productId, cancellationToken);
        return HandleResult(result);
    }
}
