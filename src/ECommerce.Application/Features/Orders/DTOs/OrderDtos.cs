using ECommerce.Application.Common.Models;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Orders.DTOs;

public record OrderItemDto(
    Guid Id,
    Guid? ProductVariantId,
    string ProductName,
    string VariantName,
    string SKU,
    string? ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public record OrderStatusHistoryDto(
    Guid Id,
    OrderStatus? OldStatus,
    OrderStatus NewStatus,
    string ChangedByName,
    string? Comment,
    DateTime CreatedAt);

public record PaymentSummaryDto(
    Guid Id,
    string? TransactionId,
    PaymentProvider Provider,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    DateTime? PaidAt);

public record OrderListDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    string CustomerName,
    OrderStatus Status,
    int ItemCount,
    decimal TotalAmount,
    string? ShippingMethodName,
    DateTime CreatedAt);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    string CustomerName,
    string CustomerEmail,
    OrderStatus Status,
    decimal SubTotal,
    decimal ShippingCost,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? CouponCode,
    string ShippingFullName,
    string ShippingPhoneNumber,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string ShippingState,
    string ShippingPostalCode,
    string ShippingCountry,
    string? ShippingMethodName,
    string? Notes,
    DateTime CreatedAt,
    List<OrderItemDto> Items,
    List<OrderStatusHistoryDto> StatusHistory,
    PaymentSummaryDto? Payment);

public record CheckoutDto(
    string ShippingFullName,
    string ShippingPhoneNumber,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string ShippingState,
    string ShippingPostalCode,
    string ShippingCountry,
    Guid ShippingMethodId,
    string? CouponCode = null,
    PaymentProvider PaymentProvider = PaymentProvider.Mock,
    string? Notes = null);

public record ChangeOrderStatusDto(
    OrderStatus NewStatus,
    string? Comment = null);

public record CancelOrderDto(
    string? Reason = null);

public class OrderFilterParams : PaginationParams
{
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? UserId { get; set; }
}
