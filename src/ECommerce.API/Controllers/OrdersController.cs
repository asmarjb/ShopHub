using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Orders.DTOs;
using ECommerce.Application.Features.Orders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
[Route("api/orders")]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly ICurrentUserService _currentUserService;

    public OrdersController(IOrderService orderService, ICurrentUserService currentUserService)
    {
        _orderService = orderService;
        _currentUserService = currentUserService;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("Manager"))
        {
            return BadRequest(new { message = "Administrators and managers cannot place orders. Please use a customer account." });
        }

        var result = await _orderService.CheckoutAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetUserOrdersAsync(pagination, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        var currentUserId = _currentUserService.UserId;
        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

        if (result.Data.UserId != currentUserId && !isAdminOrManager)
        {
            return Forbid();
        }

        return Ok(result.Data);
    }

    [AllowAnonymous]
    [HttpGet("track/{orderNumber}")]
    public async Task<IActionResult> TrackOrderByNumber(string orderNumber, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrderByNumberAsync(orderNumber, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderDto? dto, CancellationToken cancellationToken)
    {
        var result = await _orderService.CancelOrderAsync(id, dto?.Reason, cancellationToken);
        return HandleResult(result);
    }
}
