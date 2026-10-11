using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Features.Coupons.Services;
using ECommerce.Application.Features.Orders.DTOs;
using ECommerce.Application.Features.Orders.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Tests;

public class OrderStockTests
{
    [Fact]
    public async Task Checkout_ShouldDecreaseVariantStock_AndRecordStockMovement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using var context = TestDbContextFactory.Create(nameof(Checkout_ShouldDecreaseVariantStock_AndRecordStockMovement), userId);

        var user = new AppUser
        {
            Id = userId,
            Email = "customer@test.com",
            UserName = "customer@test.com",
            FirstName = "Ali",
            LastName = "Aliyev"
        };
        context.Users.Add(user);

        var category = new Category { Id = Guid.NewGuid(), Name = "Elektronika", Slug = "elektronika" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Smart Saat",
            Slug = "smart-saat",
            SKU = "WATCH-MAIN",
            BasePrice = 100m,
            CategoryId = category.Id
        };
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            SKU = "WATCH-BLK-01",
            Name = "Black",
            Price = 100m,
            StockQuantity = 10
        };
        var shipping = new ShippingMethod
        {
            Id = Guid.NewGuid(),
            Name = "Standart",
            Price = 5m,
            EstimatedDeliveryDays = "2-3 business days",
            IsActive = true
        };

        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductVariantId = variant.Id,
            Quantity = 2
        };

        context.Categories.Add(category);
        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        context.ShippingMethods.Add(shipping);
        context.Carts.Add(cart);
        context.CartItems.Add(cartItem);
        await context.SaveChangesAsync();

        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.UserId).Returns(userId);

        var mockCouponService = new Mock<ICouponService>();
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService
            .Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResultDto(true, "MOCK-TX-123", null));

        var mockEmail = new Mock<IEmailService>();

        var orderService = new OrderService(
            context,
            mockCurrentUserService.Object,
            mockCouponService.Object,
            mockPaymentService.Object,
            mockEmail.Object);

        var checkoutDto = new CheckoutDto(
            ShippingFullName: "Ali Aliyev",
            ShippingPhoneNumber: "+994501234567",
            ShippingAddressLine1: "Nizami 10",
            ShippingAddressLine2: null,
            ShippingCity: "Baku",
            ShippingState: "Absheron",
            ShippingPostalCode: "AZ1000",
            ShippingCountry: "Azerbaijan",
            ShippingMethodId: shipping.Id,
            CouponCode: null,
            PaymentProvider: PaymentProvider.Mock,
            Notes: null
        );

        // Act
        var result = await orderService.CheckoutAsync(checkoutDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalAmount.Should().Be(205m); // 2 * 100 + 5 shipping

        // Check variant stock deduction
        var updatedVariant = await context.ProductVariants.FindAsync(variant.Id);
        updatedVariant!.StockQuantity.Should().Be(8); // 10 - 2 = 8

        // Check stock movement logged
        var movements = context.StockMovements.Where(sm => sm.ProductVariantId == variant.Id).ToList();
        movements.Should().HaveCount(1);
        movements[0].Quantity.Should().Be(-2);
        movements[0].MovementType.Should().Be(StockMovementType.Out);
    }
}
