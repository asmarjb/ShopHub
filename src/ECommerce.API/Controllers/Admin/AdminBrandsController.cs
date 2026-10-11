using ECommerce.Application.Features.Brands.DTOs;
using ECommerce.Application.Features.Brands.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/brands")]
public class AdminBrandsController : BaseApiController
{
    private readonly IBrandService _brandService;

    public AdminBrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpPost]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto dto, CancellationToken cancellationToken)
    {
        var result = await _brandService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandDto dto, CancellationToken cancellationToken)
    {
        var result = await _brandService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> DeleteBrand(Guid id, CancellationToken cancellationToken)
    {
        var result = await _brandService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
