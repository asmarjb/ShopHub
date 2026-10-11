using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.StockMovements.DTOs;

public record StockMovementDto(
    Guid Id,
    Guid ProductVariantId,
    string ProductName,
    string VariantName,
    string SKU,
    StockMovementType MovementType,
    int Quantity,
    int RemainingStock,
    string? ReferenceNumber,
    string? Reason,
    DateTime CreatedAt);

public record CreateStockAdjustmentDto(
    Guid ProductVariantId,
    int Quantity,
    string? ReferenceNumber,
    string Reason);
