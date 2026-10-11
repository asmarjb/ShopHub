using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Features.Orders.DTOs;
using ECommerce.Application.Features.Orders.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/orders")]
public class AdminOrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly ICsvExportService _csvExportService;

    public AdminOrdersController(
        IOrderService orderService,
        ICsvExportService csvExportService)
    {
        _orderService = orderService;
        _csvExportService = csvExportService;
    }

    [HttpGet]
    [HasPermission(Permissions.OrderView)]
    public async Task<IActionResult> GetOrders([FromQuery] OrderFilterParams filter, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetAdminOrdersAsync(filter, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.OrderView)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/status")]
    [HasPermission(Permissions.OrderManage)]
    public async Task<IActionResult> ChangeOrderStatus(Guid id, [FromBody] ChangeOrderStatusDto dto, CancellationToken cancellationToken)
    {
        var result = await _orderService.ChangeStatusAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("export-csv")]
    [HasPermission(Permissions.OrderView)]
    public async Task<IActionResult> ExportOrdersCsv(CancellationToken cancellationToken)
    {
        var bytes = await _csvExportService.ExportOrdersToCsvAsync(cancellationToken);
        var fileName = $"orders-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
