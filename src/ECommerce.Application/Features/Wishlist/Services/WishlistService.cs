using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Wishlist.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Wishlist.Services;

public class WishlistService : IWishlistService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public WishlistService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<WishlistItemDto>>> GetUserWishlistAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<IReadOnlyList<WishlistItemDto>>(Error.Unauthorized());

        var items = await _context.WishlistItems
            .Include(w => w.Product).ThenInclude(p => p.Images)
            .AsNoTracking()
            .Where(w => w.UserId == userId.Value)
            .OrderByDescending(w => w.AddedAt)
            .Select(w => new WishlistItemDto(
                w.Id,
                w.ProductId,
                w.Product.Name,
                w.Product.Slug,
                w.Product.BasePrice,
                w.Product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                w.Product.AverageRating,
                w.Product.ReviewCount,
                w.AddedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<WishlistItemDto>>(items);
    }

    public async Task<Result<WishlistItemDto>> AddToWishlistAsync(AddToWishlistDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<WishlistItemDto>(Error.Unauthorized());

        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == dto.ProductId, cancellationToken);
        if (product == null)
            return Result.Failure<WishlistItemDto>(Error.NotFound("Product", dto.ProductId));

        var exists = await _context.WishlistItems.AnyAsync(w => w.UserId == userId.Value && w.ProductId == dto.ProductId, cancellationToken);
        if (exists)
            return Result.Failure<WishlistItemDto>("Product is already in your wishlist.");

        var item = new WishlistItem
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            ProductId = dto.ProductId,
            AddedAt = DateTime.UtcNow
        };

        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new WishlistItemDto(
            item.Id,
            product.Id,
            product.Name,
            product.Slug,
            product.BasePrice,
            product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
            product.AverageRating,
            product.ReviewCount,
            item.AddedAt));
    }

    public async Task<Result> RemoveFromWishlistAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure(Error.Unauthorized());

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId.Value && w.ProductId == productId, cancellationToken);

        if (item != null)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result<bool>> IsInWishlistAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Success(false);

        var exists = await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId.Value && w.ProductId == productId, cancellationToken);

        return Result.Success(exists);
    }
}
