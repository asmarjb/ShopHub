namespace ECommerce.Application.Features.Cart.DTOs;

public record CartItemDto(
    Guid Id,
    Guid ProductVariantId,
    string ProductName,
    string VariantName,
    string SKU,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    int AvailableStock);

public record CartDto(
    Guid Id,
    Guid? UserId,
    string? AnonymousId,
    List<CartItemDto> Items,
    int TotalItems,
    decimal SubTotal);

public record AddToCartDto
{
    public Guid ProductVariantId { get; init; }
    public Guid? VariantId { get; init; }
    public Guid? ProductId { get; init; }
    public int Quantity { get; init; } = 1;

    public AddToCartDto() { }

    public AddToCartDto(Guid productVariantId, int quantity = 1)
    {
        ProductVariantId = productVariantId;
        Quantity = quantity;
    }
}

public record UpdateCartItemDto(
    int Quantity);
