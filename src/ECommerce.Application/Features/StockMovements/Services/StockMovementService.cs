using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.StockMovements.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.StockMovements.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StockMovementService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<StockMovementDto>>> GetMovementsAsync(Guid? variantId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _context.StockMovements
            .Include(sm => sm.ProductVariant).ThenInclude(v => v.Product)
            .AsNoTracking();

        if (variantId.HasValue)
            query = query.Where(sm => sm.ProductVariantId == variantId.Value);

        query = query.OrderByDescending(sm => sm.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(sm => new StockMovementDto(
                sm.Id,
                sm.ProductVariantId,
                sm.ProductVariant.Product.Name,
                sm.ProductVariant.Name,
                sm.ProductVariant.SKU,
                sm.MovementType,
                sm.Quantity,
                sm.RemainingStock,
                sm.ReferenceNumber,
                sm.Reason,
                sm.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<StockMovementDto>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize));
    }

    public async Task<Result<StockMovementDto>> RecordAdjustmentAsync(CreateStockAdjustmentDto dto, CancellationToken cancellationToken = default)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId, cancellationToken);

        if (variant == null)
            return Result.Failure<StockMovementDto>(Error.NotFound("ProductVariant", dto.ProductVariantId));

        var newQuantity = variant.StockQuantity + dto.Quantity;
        if (newQuantity < 0)
            return Result.Failure<StockMovementDto>($"Cannot adjust stock below zero. Current stock is {variant.StockQuantity}.");

        variant.StockQuantity = newQuantity;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductVariantId = variant.Id,
            MovementType = StockMovementType.Adjustment,
            Quantity = dto.Quantity,
            RemainingStock = variant.StockQuantity,
            ReferenceNumber = dto.ReferenceNumber,
            Reason = dto.Reason,
            CreatedByUserId = _currentUserService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new StockMovementDto(
            movement.Id,
            movement.ProductVariantId,
            variant.Product.Name,
            variant.Name,
            variant.SKU,
            movement.MovementType,
            movement.Quantity,
            movement.RemainingStock,
            movement.ReferenceNumber,
            movement.Reason,
            movement.CreatedAt));
    }
}
