using ECommerce.Application.Features.Reviews.DTOs;
using ECommerce.Application.Features.Reviews.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Route("api/reviews")]
public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetProductReviews(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetApprovedProductReviewsAsync(productId, cancellationToken);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto, CancellationToken cancellationToken)
    {
        var result = await _reviewService.CreateReviewAsync(dto, cancellationToken);
        return HandleResult(result);
    }
}
