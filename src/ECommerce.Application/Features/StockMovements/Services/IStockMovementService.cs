using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.StockMovements.DTOs;

namespace ECommerce.Application.Features.StockMovements.Services;

public interface IStockMovementService
{
    Task<Result<PagedResult<StockMovementDto>>> GetMovementsAsync(Guid? variantId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<StockMovementDto>> RecordAdjustmentAsync(CreateStockAdjustmentDto dto, CancellationToken cancellationToken = default);
}
