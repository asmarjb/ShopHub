namespace ECommerce.Application.Features.Shipping.DTOs;

public record ShippingMethodDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string EstimatedDeliveryDays,
    bool IsActive);

public record CreateShippingMethodDto(
    string Name,
    string? Description,
    decimal Price,
    string EstimatedDeliveryDays,
    bool IsActive = true);

public record UpdateShippingMethodDto(
    string Name,
    string? Description,
    decimal Price,
    string EstimatedDeliveryDays,
    bool IsActive);
