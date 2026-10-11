using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Returns.DTOs;

public record ReturnRequestDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    Guid OrderItemId,
    string ProductName,
    string VariantName,
    Guid UserId,
    string CustomerName,
    string Reason,
    string? CustomerNote,
    string? AdminNote,
    ReturnStatus Status,
    DateTime RequestedAt,
    DateTime? ProcessedAt,
    string? ProcessedBy);

public record CreateReturnRequestDto(
    Guid OrderId,
    Guid OrderItemId,
    string Reason,
    string? CustomerNote);

public record ProcessReturnRequestDto(
    ReturnStatus Status,
    string? AdminNote);
