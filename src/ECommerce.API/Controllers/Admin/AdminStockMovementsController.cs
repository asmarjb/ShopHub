using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.StockMovements.DTOs;
using ECommerce.Application.Features.StockMovements.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/stock-movements")]
public class AdminStockMovementsController : BaseApiController
{
    private readonly IStockMovementService _stockMovementService;

    public AdminStockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpGet]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> GetStockMovements(
        [FromQuery] Guid? variantId,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var result = await _stockMovementService.GetMovementsAsync(variantId, pagination, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("adjust")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> RecordAdjustment(
        [FromBody] CreateStockAdjustmentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _stockMovementService.RecordAdjustmentAsync(dto, cancellationToken);
        return HandleResult(result);
    }
}
