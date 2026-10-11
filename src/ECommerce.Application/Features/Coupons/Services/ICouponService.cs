using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Coupons.DTOs;

namespace ECommerce.Application.Features.Coupons.Services;

public interface ICouponService
{
    Task<Result<IReadOnlyList<CouponDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CouponDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CouponDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<CouponDiscountResultDto>> ValidateAndApplyAsync(ApplyCouponDto dto, CancellationToken cancellationToken = default);
    Task<Result<CouponDto>> CreateAsync(CreateCouponDto dto, CancellationToken cancellationToken = default);
    Task<Result<CouponDto>> UpdateAsync(Guid id, UpdateCouponDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
