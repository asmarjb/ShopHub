using ECommerce.Application.Features.Banners.DTOs;
using ECommerce.Application.Features.Banners.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/banners")]
public class AdminBannersController : BaseApiController
{
    private readonly IBannerService _bannerService;

    public AdminBannersController(IBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    [HttpGet]
    [HasPermission(Permissions.BannerManage)]
    public async Task<IActionResult> GetAllBanners(CancellationToken cancellationToken)
    {
        var result = await _bannerService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.BannerManage)]
    public async Task<IActionResult> GetBannerById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bannerService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.BannerManage)]
    public async Task<IActionResult> CreateBanner([FromBody] CreateBannerDto dto, CancellationToken cancellationToken)
    {
        var result = await _bannerService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.BannerManage)]
    public async Task<IActionResult> UpdateBanner(Guid id, [FromBody] UpdateBannerDto dto, CancellationToken cancellationToken)
    {
        var result = await _bannerService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.BannerManage)]
    public async Task<IActionResult> DeleteBanner(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bannerService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
