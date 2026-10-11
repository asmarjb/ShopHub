using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Cart.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CartEntity = ECommerce.Domain.Entities.Cart;

namespace ECommerce.Application.Features.Cart.Services;

public class CartService : ICartService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CartService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CartDto>> GetCartAsync(string? anonymousId = null, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(anonymousId, cancellationToken);
        return Result.Success(MapToDto(cart));
    }

    public async Task<Result<CartDto>> AddItemAsync(AddToCartDto dto, string? anonymousId = null, CancellationToken cancellationToken = default)
    {
        Guid targetVariantId = dto.ProductVariantId != Guid.Empty
            ? dto.ProductVariantId
            : (dto.VariantId.HasValue && dto.VariantId.Value != Guid.Empty ? dto.VariantId.Value : Guid.Empty);

        if (targetVariantId == Guid.Empty && dto.ProductId.HasValue && dto.ProductId.Value != Guid.Empty)
        {
            var firstVariant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.ProductId == dto.ProductId.Value, cancellationToken);
            if (firstVariant != null)
            {
                targetVariantId = firstVariant.Id;
            }
        }

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == targetVariantId, cancellationToken);

        if (variant == null)
            return Result.Failure<CartDto>(Error.NotFound("ProductVariant", targetVariantId));

        if (variant.StockQuantity < dto.Quantity)
            return Result.Failure<CartDto>($"Insufficient stock. Only {variant.StockQuantity} unit(s) available.");

        var cart = await GetOrCreateCartAsync(anonymousId, cancellationToken);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == targetVariantId);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + dto.Quantity;
            if (variant.StockQuantity < newQuantity)
                return Result.Failure<CartDto>($"Cannot add more units. Total requested ({newQuantity}) exceeds available stock ({variant.StockQuantity}).");

            existingItem.Quantity = newQuantity;
        }
        else
        {
            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductVariantId = targetVariantId,
                Quantity = dto.Quantity
            };
            _context.CartItems.Add(newItem);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetCartAsync(cart.AnonymousId ?? anonymousId, cancellationToken);
    }

    public async Task<Result<CartDto>> UpdateItemQuantityAsync(Guid cartItemId, UpdateCartItemDto dto, string? anonymousId = null, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(anonymousId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

        if (item == null)
            return Result.Failure<CartDto>(Error.NotFound("CartItem", cartItemId));

        var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.ProductVariantId, cancellationToken);
        if (variant == null)
            return Result.Failure<CartDto>("Associated product variant not found.");

        if (dto.Quantity > variant.StockQuantity)
            return Result.Failure<CartDto>($"Requested quantity ({dto.Quantity}) exceeds available stock ({variant.StockQuantity}).");

        item.Quantity = dto.Quantity;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetCartAsync(cart.AnonymousId ?? anonymousId, cancellationToken);
    }

    public async Task<Result<CartDto>> RemoveItemAsync(Guid cartItemId, string? anonymousId = null, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(anonymousId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return await GetCartAsync(cart.AnonymousId ?? anonymousId, cancellationToken);
    }

    public async Task<Result> ClearCartAsync(string? anonymousId = null, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(anonymousId, cancellationToken);
        if (cart.Items.Any())
        {
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> MergeCartAsync(string anonymousId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(anonymousId))
            return Result.Success();

        var anonymousCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.AnonymousId == anonymousId, cancellationToken);

        if (anonymousCart == null || !anonymousCart.Items.Any())
            return Result.Success();

        var userCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (userCart == null)
        {
            // Simply reassign anonymous cart to user
            anonymousCart.UserId = userId;
            anonymousCart.AnonymousId = null;
        }
        else
        {
            // Merge items
            foreach (var anonItem in anonymousCart.Items)
            {
                var existing = userCart.Items.FirstOrDefault(i => i.ProductVariantId == anonItem.ProductVariantId);
                if (existing != null)
                {
                    existing.Quantity += anonItem.Quantity;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = userCart.Id,
                        ProductVariantId = anonItem.ProductVariantId,
                        Quantity = anonItem.Quantity
                    };
                    _context.CartItems.Add(newItem);
                }
            }

            _context.Carts.Remove(anonymousCart);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<CartEntity> GetOrCreateCartAsync(string? anonymousId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        CartEntity? cart = null;

        if (userId.HasValue)
        {
            cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(anonymousId))
        {
            cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.AnonymousId == anonymousId, cancellationToken);
        }

        if (cart == null)
        {
            cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AnonymousId = userId.HasValue ? null : (anonymousId ?? Guid.NewGuid().ToString("N"))
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return cart;
    }

    private static CartDto MapToDto(CartEntity cart)
    {
        var items = cart.Items.Select(i => new CartItemDto(
            i.Id,
            i.ProductVariantId,
            i.ProductVariant?.Product?.Name ?? "Item",
            i.ProductVariant?.Name ?? "Standard",
            i.ProductVariant?.SKU ?? "",
            i.ProductVariant?.ImageUrl,
            i.ProductVariant?.Price ?? 0m,
            i.Quantity,
            (i.ProductVariant?.Price ?? 0m) * i.Quantity,
            i.ProductVariant?.StockQuantity ?? 0)).ToList();

        var totalItems = items.Sum(i => i.Quantity);
        var subTotal = items.Sum(i => i.TotalPrice);

        return new CartDto(
            cart.Id,
            cart.UserId,
            cart.AnonymousId,
            items,
            totalItems,
            subTotal);
    }
}
