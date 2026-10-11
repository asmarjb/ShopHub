using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Reviews.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Reviews.Services;

public class ReviewService : IReviewService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ReviewService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetApprovedProductReviewsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ReviewDto>>(reviews);
    }

    public async Task<Result<PagedResult<ReviewDto>>> GetPendingReviewsAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .AsNoTracking()
            .Where(r => !r.IsApproved)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<ReviewDto>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize));
    }

    public async Task<Result<ReviewDto>> CreateReviewAsync(CreateReviewDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<ReviewDto>(Error.Unauthorized());

        // 1. Verify that the order exists, belongs to this customer, is Delivered, and contains this product
        var order = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId.Value, cancellationToken);

        if (order == null)
            return Result.Failure<ReviewDto>("Matching order not found.");

        if (order.Status != OrderStatus.Delivered)
            return Result.Failure<ReviewDto>("You can only review products from delivered orders ('Verified Purchase').");

        var containsProduct = order.Items.Any(i => i.ProductVariant != null && i.ProductVariant.ProductId == dto.ProductId);
        if (!containsProduct)
            return Result.Failure<ReviewDto>("The specified order does not contain this product.");

        // 2. Prevent duplicate reviews for the same order purchase
        var duplicate = await _context.Reviews.AnyAsync(r =>
            r.UserId == userId.Value && r.ProductId == dto.ProductId && r.OrderId == dto.OrderId, cancellationToken);

        if (duplicate)
            return Result.Failure<ReviewDto>("You have already reviewed this product for this order purchase.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId, cancellationToken);

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            UserId = userId.Value,
            OrderId = dto.OrderId,
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment,
            IsApproved = false, // Pending moderation
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReviewDto(
            review.Id,
            review.ProductId,
            product?.Name ?? "",
            review.UserId,
            $"{user?.FirstName} {user?.LastName}",
            review.OrderId,
            review.Rating,
            review.Title,
            review.Comment,
            review.IsApproved,
            review.ModeratedAt,
            review.ModeratedBy,
            review.CreatedAt));
    }

    public async Task<Result> ModerateReviewAsync(Guid reviewId, ModerateReviewDto dto, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
        if (review == null)
            return Result.Failure(Error.NotFound("Review", reviewId));

        if (dto.Approve)
        {
            review.IsApproved = true;
            review.ModeratedAt = DateTime.UtcNow;
            review.ModeratedBy = _currentUserService.UserName ?? "admin";
            await _context.SaveChangesAsync(cancellationToken);

            // Recalculate average rating for product
            await RecalculateProductRatingAsync(review.ProductId, cancellationToken);
        }
        else
        {
            // Rejected
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);
            await RecalculateProductRatingAsync(review.ProductId, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
        if (review == null)
            return Result.Failure(Error.NotFound("Review", reviewId));

        var productId = review.ProductId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateProductRatingAsync(productId, cancellationToken);
        return Result.Success();
    }

    private async Task RecalculateProductRatingAsync(Guid productId, CancellationToken cancellationToken)
    {
        var approvedReviews = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync(cancellationToken);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product != null)
        {
            if (approvedReviews.Any())
            {
                product.AverageRating = Math.Round(approvedReviews.Average(r => r.Rating), 1);
                product.ReviewCount = approvedReviews.Count;
            }
            else
            {
                product.AverageRating = 0;
                product.ReviewCount = 0;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static ReviewDto MapToDto(Review r) => new(
        r.Id,
        r.ProductId,
        r.Product?.Name ?? "",
        r.UserId,
        $"{r.User?.FirstName} {r.User?.LastName}",
        r.OrderId,
        r.Rating,
        r.Title,
        r.Comment,
        r.IsApproved,
        r.ModeratedAt,
        r.ModeratedBy,
        r.CreatedAt);
}
