using ECommerce.Application.Features.Cart.DTOs;
using ECommerce.Application.Features.Cart.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Route("api/cart")]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var anonymousId = GetAnonymousCartId();
        var result = await _cartService.GetCartAsync(anonymousId, cancellationToken);
        if (result.IsSuccess)
        {
            SetAnonymousCartHeader(result.Data.AnonymousId);
        }
        return HandleResult(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto, CancellationToken cancellationToken)
    {
        var anonymousId = GetAnonymousCartId();
        var result = await _cartService.AddItemAsync(dto, anonymousId, cancellationToken);
        if (result.IsSuccess)
        {
            SetAnonymousCartHeader(result.Data.AnonymousId);
        }
        return HandleResult(result);
    }

    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItemQuantity(Guid itemId, [FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken)
    {
        var anonymousId = GetAnonymousCartId();
        var result = await _cartService.UpdateItemQuantityAsync(itemId, dto, anonymousId, cancellationToken);
        if (result.IsSuccess)
        {
            SetAnonymousCartHeader(result.Data.AnonymousId);
        }
        return HandleResult(result);
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
    {
        var anonymousId = GetAnonymousCartId();
        var result = await _cartService.RemoveItemAsync(itemId, anonymousId, cancellationToken);
        if (result.IsSuccess)
        {
            SetAnonymousCartHeader(result.Data.AnonymousId);
        }
        return HandleResult(result);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var anonymousId = GetAnonymousCartId();
        var result = await _cartService.ClearCartAsync(anonymousId, cancellationToken);
        return HandleResult(result);
    }

    private string? GetAnonymousCartId()
    {
        if (Request.Headers.TryGetValue("X-Anonymous-Cart-Id", out var headerVal) && !string.IsNullOrWhiteSpace(headerVal))
        {
            return headerVal.ToString();
        }

        var queryVal = Request.Query["anonymousId"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(queryVal))
        {
            return queryVal;
        }

        return null;
    }

    private void SetAnonymousCartHeader(string? anonymousId)
    {
        if (!string.IsNullOrWhiteSpace(anonymousId))
        {
            Response.Headers["X-Anonymous-Cart-Id"] = anonymousId;
        }
    }
}
