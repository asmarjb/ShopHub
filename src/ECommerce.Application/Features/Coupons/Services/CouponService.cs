using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Coupons.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Coupons.Services;

public class CouponService : ICouponService
{
    private readonly IApplicationDbContext _context;

    public CouponService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CouponDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var coupons = await _context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => MapToDto(c))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<CouponDto>>(coupons);
    }

    public async Task<Result<CouponDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (coupon == null)
            return Result.Failure<CouponDto>(Error.NotFound("Coupon", id));

        return Result.Success(MapToDto(coupon));
    }

    public async Task<Result<CouponDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.Trim().ToUpper(), cancellationToken);

        if (coupon == null)
            return Result.Failure<CouponDto>(Error.NotFound("Coupon", code));

        return Result.Success(MapToDto(coupon));
    }

    public async Task<Result<CouponDiscountResultDto>> ValidateAndApplyAsync(ApplyCouponDto dto, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == dto.Code.Trim().ToUpper(), cancellationToken);

        if (coupon == null)
            return Result.Failure<CouponDiscountResultDto>("Coupon code is invalid.");

        if (!coupon.IsActive)
            return Result.Failure<CouponDiscountResultDto>("This coupon is currently inactive.");

        if (coupon.StartDate.HasValue && DateTime.UtcNow < coupon.StartDate.Value)
            return Result.Failure<CouponDiscountResultDto>("This coupon promotion has not started yet.");

        if (coupon.ExpiryDate.HasValue && DateTime.UtcNow > coupon.ExpiryDate.Value)
            return Result.Failure<CouponDiscountResultDto>("This coupon code has expired.");

        if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value)
            return Result.Failure<CouponDiscountResultDto>("This coupon has reached its maximum global usage limit.");

        if (coupon.MinOrderAmount.HasValue && dto.SubTotal < coupon.MinOrderAmount.Value)
            return Result.Failure<CouponDiscountResultDto>($"Order subtotal must be at least ${coupon.MinOrderAmount.Value:F2} to apply this coupon.");

        decimal discount = 0m;
        if (coupon.CouponType == CouponType.Percentage)
        {
            discount = dto.SubTotal * (coupon.DiscountValue / 100m);
            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
            {
                discount = coupon.MaxDiscountAmount.Value;
            }
        }
        else // FixedAmount
        {
            discount = Math.Min(coupon.DiscountValue, dto.SubTotal);
        }

        var finalTotal = Math.Max(0m, dto.SubTotal - discount);

        return Result.Success(new CouponDiscountResultDto(
            coupon.Code,
            coupon.CouponType,
            Math.Round(discount, 2),
            Math.Round(finalTotal, 2)));
    }

    public async Task<Result<CouponDto>> CreateAsync(CreateCouponDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedCode = dto.Code.Trim().ToUpper();
        if (await _context.Coupons.AnyAsync(c => c.Code == normalizedCode, cancellationToken))
            return Result.Failure<CouponDto>($"Coupon with code '{normalizedCode}' already exists.");

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Description = dto.Description,
            CouponType = dto.CouponType,
            DiscountValue = dto.DiscountValue,
            MinOrderAmount = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            StartDate = dto.StartDate,
            ExpiryDate = dto.ExpiryDate,
            UsageLimit = dto.UsageLimit,
            IsActive = dto.IsActive
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(coupon));
    }

    public async Task<Result<CouponDto>> UpdateAsync(Guid id, UpdateCouponDto dto, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (coupon == null)
            return Result.Failure<CouponDto>(Error.NotFound("Coupon", id));

        coupon.Description = dto.Description;
        coupon.CouponType = dto.CouponType;
        coupon.DiscountValue = dto.DiscountValue;
        coupon.MinOrderAmount = dto.MinOrderAmount;
        coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        coupon.StartDate = dto.StartDate;
        coupon.ExpiryDate = dto.ExpiryDate;
        coupon.UsageLimit = dto.UsageLimit;
        coupon.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(MapToDto(coupon));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (coupon == null)
            return Result.Failure(Error.NotFound("Coupon", id));

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static CouponDto MapToDto(Coupon c) => new(
        c.Id,
        c.Code,
        c.Description,
        c.CouponType,
        c.DiscountValue,
        c.MinOrderAmount,
        c.MaxDiscountAmount,
        c.StartDate,
        c.ExpiryDate,
        c.UsageLimit,
        c.TimesUsed,
        c.IsActive,
        c.IsValid);
}
