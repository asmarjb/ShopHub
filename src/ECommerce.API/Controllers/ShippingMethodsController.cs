using ECommerce.Application.Features.Shipping.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Route("api/shipping-methods")]
public class ShippingMethodsController : BaseApiController
{
    private readonly IShippingService _shippingService;

    public ShippingMethodsController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _shippingService.GetAllAsync(includeInactive: false, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _shippingService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
