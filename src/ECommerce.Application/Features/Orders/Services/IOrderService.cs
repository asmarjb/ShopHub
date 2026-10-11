using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Orders.DTOs;

namespace ECommerce.Application.Features.Orders.Services;

public interface IOrderService
{
    Task<Result<OrderDetailDto>> CheckoutAsync(CheckoutDto dto, CancellationToken cancellationToken = default);
    Task<Result<OrderDetailDto>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<OrderDetailDto>> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderListDto>>> GetUserOrdersAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderListDto>>> GetAdminOrdersAsync(OrderFilterParams filter, CancellationToken cancellationToken = default);
    Task<Result<OrderDetailDto>> ChangeStatusAsync(Guid orderId, ChangeOrderStatusDto dto, CancellationToken cancellationToken = default);
    Task<Result> CancelOrderAsync(Guid orderId, string? reason = null, CancellationToken cancellationToken = default);
}
