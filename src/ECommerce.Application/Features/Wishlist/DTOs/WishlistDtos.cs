namespace ECommerce.Application.Features.Wishlist.DTOs;

public record WishlistItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    decimal BasePrice,
    string? PrimaryImageUrl,
    double AverageRating,
    int ReviewCount,
    DateTime AddedAt);

public record AddToWishlistDto(
    Guid ProductId);
