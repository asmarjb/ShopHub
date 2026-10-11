using ECommerce.Application.Features.Shipping.DTOs;
using ECommerce.Application.Features.Shipping.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/shipping-methods")]
public class AdminShippingMethodsController : BaseApiController
{
    private readonly IShippingService _shippingService;

    public AdminShippingMethodsController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    [HttpGet]
    [HasPermission(Permissions.ShippingManage)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _shippingService.GetAllAsync(includeInactive: true, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.ShippingManage)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _shippingService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.ShippingManage)]
    public async Task<IActionResult> Create([FromBody] CreateShippingMethodDto dto, CancellationToken cancellationToken)
    {
        var result = await _shippingService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.ShippingManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShippingMethodDto dto, CancellationToken cancellationToken)
    {
        var result = await _shippingService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.ShippingManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _shippingService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
