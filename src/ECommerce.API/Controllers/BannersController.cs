using ECommerce.Application.Features.Banners.Services;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

public class BannersController : BaseApiController
{
    private readonly IBannerService _bannerService;

    public BannersController(IBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBanners([FromQuery] BannerPosition? position = null, CancellationToken cancellationToken = default)
    {
        var result = await _bannerService.GetActiveBannersAsync(position, cancellationToken);
        return HandleResult(result);
    }
}
