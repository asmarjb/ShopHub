using ECommerce.Application.Features.Coupons.DTOs;
using ECommerce.Application.Features.Coupons.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/coupons")]
public class AdminCouponsController : BaseApiController
{
    private readonly ICouponService _couponService;

    public AdminCouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    [HasPermission(Permissions.CouponManage)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _couponService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.CouponManage)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.CouponManage)]
    public async Task<IActionResult> Create([FromBody] CreateCouponDto dto, CancellationToken cancellationToken)
    {
        var result = await _couponService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.CouponManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponDto dto, CancellationToken cancellationToken)
    {
        var result = await _couponService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.CouponManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
