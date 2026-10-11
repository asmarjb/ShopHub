using ECommerce.Application.Features.Coupons.DTOs;
using ECommerce.Application.Features.Coupons.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Route("api/coupons")]
public class CouponsController : BaseApiController
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ApplyCouponDto dto, CancellationToken cancellationToken)
    {
        var result = await _couponService.ValidateAndApplyAsync(dto, cancellationToken);
        return HandleResult(result);
    }
}
