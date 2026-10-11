using ECommerce.Application.Features.Coupons.DTOs;
using ECommerce.Application.Features.Coupons.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests;

public class CouponValidationTests
{
    [Fact]
    public async Task ValidateCoupon_PercentageDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(ValidateCoupon_PercentageDiscount_ShouldCalculateCorrectly));

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = "SALE20",
            CouponType = CouponType.Percentage,
            DiscountValue = 20m, // 20%
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(5),
            IsActive = true,
            UsageLimit = 100,
            TimesUsed = 5
        };
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();

        var couponService = new CouponService(context);

        // Act
        var result = await couponService.ValidateAndApplyAsync(new ApplyCouponDto("SALE20", 200m));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.DiscountAmount.Should().Be(40m); // 20% of 200 = 40
        result.Data.FinalTotal.Should().Be(160m);   // 200 - 40 = 160
    }

    [Fact]
    public async Task ValidateCoupon_ExpiredCoupon_ShouldFail()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(ValidateCoupon_ExpiredCoupon_ShouldFail));

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = "OLDCODE",
            CouponType = CouponType.FixedAmount,
            DiscountValue = 15m,
            StartDate = DateTime.UtcNow.AddDays(-10),
            ExpiryDate = DateTime.UtcNow.AddDays(-1), // Expired
            IsActive = true
        };
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync();

        var couponService = new CouponService(context);

        // Act
        var result = await couponService.ValidateAndApplyAsync(new ApplyCouponDto("OLDCODE", 100m));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.ToLower().Should().Contain("expired");
    }
}
