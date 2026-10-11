using ECommerce.Application.Features.Brands.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

public class BrandsController : BaseApiController
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBrands([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await _brandService.GetAllAsync(includeInactive, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBrandById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _brandService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBrandBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _brandService.GetBySlugAsync(slug, cancellationToken);
        return HandleResult(result);
    }
}
