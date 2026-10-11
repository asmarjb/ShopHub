using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Returns.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Returns.Services;

public class ReturnRequestService : IReturnRequestService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ReturnRequestService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<ReturnRequestDto>>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _context.ReturnRequests
            .Include(r => r.Order)
            .Include(r => r.OrderItem)
            .Include(r => r.User)
            .AsNoTracking()
            .OrderByDescending(r => r.RequestedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<ReturnRequestDto>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize));
    }

    public async Task<Result<IReadOnlyList<ReturnRequestDto>>> GetUserReturnRequestsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<IReadOnlyList<ReturnRequestDto>>(Error.Unauthorized());

        var items = await _context.ReturnRequests
            .Include(r => r.Order)
            .Include(r => r.OrderItem)
            .Include(r => r.User)
            .AsNoTracking()
            .Where(r => r.UserId == userId.Value)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ReturnRequestDto>>(items);
    }

    public async Task<Result<ReturnRequestDto>> CreateAsync(CreateReturnRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<ReturnRequestDto>(Error.Unauthorized());

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId.Value, cancellationToken);

        if (order == null)
            return Result.Failure<ReturnRequestDto>("Matching order not found.");

        if (order.Status != OrderStatus.Delivered)
            return Result.Failure<ReturnRequestDto>("Returns can only be requested for orders with 'Delivered' status.");

        var item = order.Items.FirstOrDefault(i => i.Id == dto.OrderItemId);
        if (item == null)
            return Result.Failure<ReturnRequestDto>("Order item not found in the specified order.");

        var existing = await _context.ReturnRequests
            .AnyAsync(r => r.OrderId == dto.OrderId && r.OrderItemId == dto.OrderItemId && r.Status != ReturnStatus.Rejected, cancellationToken);

        if (existing)
            return Result.Failure<ReturnRequestDto>("A return request for this order item is already under review or processed.");

        var returnRequest = new ReturnRequest
        {
            Id = Guid.NewGuid(),
            OrderId = dto.OrderId,
            OrderItemId = dto.OrderItemId,
            UserId = userId.Value,
            Reason = dto.Reason,
            CustomerNote = dto.CustomerNote,
            Status = ReturnStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _context.ReturnRequests.Add(returnRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(returnRequest.Id, cancellationToken);
    }

    public async Task<Result<ReturnRequestDto>> ProcessAsync(Guid id, ProcessReturnRequestDto dto, CancellationToken cancellationToken = default)
    {
        var request = await _context.ReturnRequests
            .Include(r => r.Order)
            .Include(r => r.OrderItem)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (request == null)
            return Result.Failure<ReturnRequestDto>(Error.NotFound("ReturnRequest", id));

        request.Status = dto.Status;
        request.AdminNote = dto.AdminNote;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedBy = _currentUserService.UserName ?? "admin";

        // If completed, optionally restore stock
        if (dto.Status == ReturnStatus.Completed && request.OrderItem.ProductVariantId.HasValue)
        {
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.OrderItem.ProductVariantId.Value, cancellationToken);
            if (variant != null)
            {
                variant.StockQuantity += request.OrderItem.Quantity;
                _context.StockMovements.Add(new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = variant.Id,
                    MovementType = StockMovementType.In,
                    Quantity = request.OrderItem.Quantity,
                    RemainingStock = variant.StockQuantity,
                    ReferenceNumber = request.Order.OrderNumber,
                    Reason = "Return completed inventory restock",
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(MapToDto(request));
    }

    private async Task<Result<ReturnRequestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await _context.ReturnRequests
            .Include(r => r.Order)
            .Include(r => r.OrderItem)
            .Include(r => r.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (request == null)
            return Result.Failure<ReturnRequestDto>(Error.NotFound("ReturnRequest", id));

        return Result.Success(MapToDto(request));
    }

    private static ReturnRequestDto MapToDto(ReturnRequest r) => new(
        r.Id,
        r.OrderId,
        r.Order?.OrderNumber ?? "",
        r.OrderItemId,
        r.OrderItem?.ProductNameSnapshot ?? "",
        r.OrderItem?.VariantNameSnapshot ?? "",
        r.UserId,
        $"{r.User?.FirstName} {r.User?.LastName}",
        r.Reason,
        r.CustomerNote,
        r.AdminNote,
        r.Status,
        r.RequestedAt,
        r.ProcessedAt,
        r.ProcessedBy);
}
