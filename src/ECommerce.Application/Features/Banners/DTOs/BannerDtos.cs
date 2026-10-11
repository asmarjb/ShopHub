using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Banners.DTOs;

public record BannerDto(
    Guid Id,
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? TargetUrl,
    BannerPosition Position,
    int DisplayOrder,
    bool IsActive,
    DateTime? StartDate,
    DateTime? EndDate);

public record CreateBannerDto(
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? TargetUrl,
    BannerPosition Position = BannerPosition.Hero,
    int DisplayOrder = 0,
    bool IsActive = true,
    DateTime? StartDate = null,
    DateTime? EndDate = null);

public record UpdateBannerDto(
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? TargetUrl,
    BannerPosition Position,
    int DisplayOrder,
    bool IsActive,
    DateTime? StartDate,
    DateTime? EndDate);
