using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Reviews.DTOs;
using ECommerce.Application.Features.Reviews.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/reviews")]
public class AdminReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public AdminReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("pending")]
    [HasPermission(Permissions.ReviewModerate)]
    public async Task<IActionResult> GetPendingReviews([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetPendingReviewsAsync(pagination, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/moderate")]
    [HasPermission(Permissions.ReviewModerate)]
    public async Task<IActionResult> ModerateReview(Guid id, [FromBody] ModerateReviewDto dto, CancellationToken cancellationToken)
    {
        var result = await _reviewService.ModerateReviewAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.ReviewModerate)]
    public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reviewService.DeleteReviewAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
