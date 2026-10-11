using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Reviews.DTOs;

namespace ECommerce.Application.Features.Reviews.Services;

public interface IReviewService
{
    Task<Result<IReadOnlyList<ReviewDto>>> GetApprovedProductReviewsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ReviewDto>>> GetPendingReviewsAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<ReviewDto>> CreateReviewAsync(CreateReviewDto dto, CancellationToken cancellationToken = default);
    Task<Result> ModerateReviewAsync(Guid reviewId, ModerateReviewDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
}
