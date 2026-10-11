namespace ECommerce.Application.Features.Reviews.DTOs;

public record ReviewDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid UserId,
    string CustomerName,
    Guid OrderId,
    int Rating,
    string Title,
    string Comment,
    bool IsApproved,
    DateTime? ModeratedAt,
    string? ModeratedBy,
    DateTime CreatedAt);

public record CreateReviewDto(
    Guid ProductId,
    Guid OrderId,
    int Rating,
    string Title,
    string Comment);

public record ModerateReviewDto(
    bool Approve);
