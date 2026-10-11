using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Coupons.DTOs;

public record CouponDto(
    Guid Id,
    string Code,
    string? Description,
    CouponType CouponType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime? StartDate,
    DateTime? ExpiryDate,
    int? UsageLimit,
    int TimesUsed,
    bool IsActive,
    bool IsValid);

public record CreateCouponDto(
    string Code,
    string? Description,
    CouponType CouponType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime? StartDate,
    DateTime? ExpiryDate,
    int? UsageLimit,
    bool IsActive = true);

public record UpdateCouponDto(
    string? Description,
    CouponType CouponType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime? StartDate,
    DateTime? ExpiryDate,
    int? UsageLimit,
    bool IsActive);

public record ApplyCouponDto(
    string Code,
    decimal SubTotal);

public record CouponDiscountResultDto(
    string Code,
    CouponType CouponType,
    decimal DiscountAmount,
    decimal FinalTotal);
