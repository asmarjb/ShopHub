using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Coupons.DTOs;
using ECommerce.Application.Features.Coupons.Services;
using ECommerce.Application.Features.Orders.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICouponService _couponService;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;

    public OrderService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICouponService couponService,
        IPaymentService paymentService,
        IEmailService emailService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _couponService = couponService;
        _paymentService = paymentService;
        _emailService = emailService;
    }

    public async Task<Result<OrderDetailDto>> CheckoutAsync(CheckoutDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<OrderDetailDto>(Error.Unauthorized("You must be logged in to complete checkout."));

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
        if (user == null)
            return Result.Failure<OrderDetailDto>(Error.NotFound("User", userId.Value));

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId.Value, cancellationToken);

        if (cart == null || !cart.Items.Any())
            return Result.Failure<OrderDetailDto>("Your shopping cart is empty.");

        var shippingMethod = await _context.ShippingMethods
            .FirstOrDefaultAsync(sm => sm.Id == dto.ShippingMethodId && sm.IsActive, cancellationToken);

        if (shippingMethod == null)
            return Result.Failure<OrderDetailDto>("Invalid or inactive shipping method selected.");

        // 1. Verify stock availability for all items
        foreach (var item in cart.Items)
        {
            if (item.ProductVariant.StockQuantity < item.Quantity)
            {
                return Result.Failure<OrderDetailDto>(
                    $"Insufficient stock for '{item.ProductVariant.Product.Name} ({item.ProductVariant.Name})'. Available: {item.ProductVariant.StockQuantity}, Requested: {item.Quantity}.");
            }
        }

        var subTotal = cart.Items.Sum(i => i.ProductVariant.Price * i.Quantity);
        decimal discountAmount = 0m;
        Coupon? appliedCoupon = null;

        // 2. Validate Coupon if provided
        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var couponValidation = await _couponService.ValidateAndApplyAsync(new ApplyCouponDto(dto.CouponCode, subTotal), cancellationToken);
            if (couponValidation.IsFailure)
            {
                return Result.Failure<OrderDetailDto>(couponValidation.Error.Message);
            }

            discountAmount = couponValidation.Data.DiscountAmount;
            appliedCoupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == dto.CouponCode.Trim().ToUpper(), cancellationToken);
            if (appliedCoupon != null)
            {
                appliedCoupon.TimesUsed++;
            }
        }

        var totalAmount = Math.Max(0m, (subTotal - discountAmount) + shippingMethod.Price);
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        // 3. Create Order with snapshot data
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            UserId = userId.Value,
            Status = OrderStatus.Pending,
            SubTotal = subTotal,
            ShippingCost = shippingMethod.Price,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            ShippingMethodId = shippingMethod.Id,
            CouponId = appliedCoupon?.Id,
            CouponCodeSnapshot = appliedCoupon?.Code,
            ShippingFullName = dto.ShippingFullName,
            ShippingPhoneNumber = dto.ShippingPhoneNumber,
            ShippingAddressLine1 = dto.ShippingAddressLine1,
            ShippingAddressLine2 = dto.ShippingAddressLine2,
            ShippingCity = dto.ShippingCity,
            ShippingState = dto.ShippingState,
            ShippingPostalCode = dto.ShippingPostalCode,
            ShippingCountry = dto.ShippingCountry,
            Notes = dto.Notes
        };

        // 4. Create OrderItems & deduct variant stock
        foreach (var cartItem in cart.Items)
        {
            var variant = cartItem.ProductVariant;
            var product = variant.Product;

            var primaryImg = await _context.ProductImages
                .Where(i => i.ProductId == product.Id && i.IsPrimary)
                .Select(i => i.ImageUrl)
                .FirstOrDefaultAsync(cancellationToken);

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductVariantId = variant.Id,
                ProductNameSnapshot = product.Name,
                VariantNameSnapshot = variant.Name,
                SKUSnapshot = variant.SKU,
                ProductImageUrlSnapshot = variant.ImageUrl ?? primaryImg,
                UnitPriceSnapshot = variant.Price,
                Quantity = cartItem.Quantity,
                TotalPrice = variant.Price * cartItem.Quantity
            };
            order.Items.Add(orderItem);

            // Deduct stock & create stock movement
            variant.StockQuantity -= cartItem.Quantity;

            _context.StockMovements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductVariantId = variant.Id,
                MovementType = StockMovementType.Out,
                Quantity = -cartItem.Quantity,
                RemainingStock = variant.StockQuantity,
                ReferenceNumber = order.OrderNumber,
                Reason = "Order placement stock deduction",
                CreatedByUserId = userId.Value,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 5. Process Payment via IPaymentService
        var paymentRequest = new PaymentRequestDto(
            order.Id,
            order.TotalAmount,
            "USD",
            dto.PaymentProvider);

        var paymentResponse = await _paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);
        if (paymentResponse.IsFailure)
        {
            return Result.Failure<OrderDetailDto>($"Payment processing failed: {paymentResponse.Error.Message}");
        }

        order.Payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            TransactionId = paymentResponse.Data.TransactionId,
            Provider = dto.PaymentProvider,
            Amount = order.TotalAmount,
            Currency = "USD",
            Status = PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow,
            PaymentDetailsJson = paymentResponse.Data.Message
        };

        // 6. Record Status Timeline History
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            OldStatus = null,
            NewStatus = OrderStatus.Pending,
            ChangedByUserId = userId.Value,
            ChangedByName = $"{user.FirstName} {user.LastName}",
            Comment = "Order placed and payment successfully captured.",
            CreatedAt = DateTime.UtcNow
        });

        // 7. Clear Shopping Cart
        _context.CartItems.RemoveRange(cart.Items);
        _context.Orders.Add(order);

        await _context.SaveChangesAsync(cancellationToken);

        // 8. Fire-and-forget Email Notification
        _ = _emailService.SendOrderConfirmationEmailAsync(
            user.Email ?? "",
            $"{user.FirstName} {user.LastName}",
            order.OrderNumber,
            order.TotalAmount,
            cancellationToken);

        return await GetOrderByIdAsync(order.Id, cancellationToken);
    }

    public async Task<Result<OrderDetailDto>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.ShippingMethod)
            .Include(o => o.Payment)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            return Result.Failure<OrderDetailDto>(Error.NotFound("Order", id));

        return Result.Success(MapToDetailDto(order));
    }

    public async Task<Result<OrderDetailDto>> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.ShippingMethod)
            .Include(o => o.Payment)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

        if (order == null)
            return Result.Failure<OrderDetailDto>(Error.NotFound("Order", orderNumber));

        return Result.Success(MapToDetailDto(order));
    }

    public async Task<Result<PagedResult<OrderListDto>>> GetUserOrdersAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.Failure<PagedResult<OrderListDto>>(Error.Unauthorized());

        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .Include(o => o.ShippingMethod)
            .AsNoTracking()
            .Where(o => o.UserId == userId.Value)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(o => new OrderListDto(
                o.Id,
                o.OrderNumber,
                o.UserId,
                $"{o.User.FirstName} {o.User.LastName}",
                o.Status,
                o.Items.Sum(i => i.Quantity),
                o.TotalAmount,
                o.ShippingMethod != null ? o.ShippingMethod.Name : null,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<OrderListDto>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize));
    }

    public async Task<Result<PagedResult<OrderListDto>>> GetAdminOrdersAsync(OrderFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .Include(o => o.ShippingMethod)
            .AsNoTracking();

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.UserId.HasValue)
            query = query.Where(o => o.UserId == filter.UserId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(search)
                                  || o.ShippingFullName.ToLower().Contains(search)
                                  || o.User.Email!.ToLower().Contains(search));
        }

        query = filter.SortBy?.ToLower() switch
        {
            "total" => filter.IsDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
            "status" => filter.IsDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => new OrderListDto(
                o.Id,
                o.OrderNumber,
                o.UserId,
                $"{o.User.FirstName} {o.User.LastName}",
                o.Status,
                o.Items.Sum(i => i.Quantity),
                o.TotalAmount,
                o.ShippingMethod != null ? o.ShippingMethod.Name : null,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<OrderListDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize));
    }

    public async Task<Result<OrderDetailDto>> ChangeStatusAsync(Guid orderId, ChangeOrderStatusDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
            return Result.Failure<OrderDetailDto>(Error.NotFound("Order", orderId));

        if (order.Status == dto.NewStatus)
            return await GetOrderByIdAsync(orderId, cancellationToken);

        var oldStatus = order.Status;
        order.Status = dto.NewStatus;

        var changedByName = _currentUserService.UserName ?? "system";
        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            OldStatus = oldStatus,
            NewStatus = dto.NewStatus,
            ChangedByUserId = _currentUserService.UserId,
            ChangedByName = changedByName,
            Comment = dto.Comment ?? $"Status changed from {oldStatus} to {dto.NewStatus}",
            CreatedAt = DateTime.UtcNow
        });

        // Write to AuditLog
        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = _currentUserService.UserId,
            UserName = changedByName,
            Action = "OrderStatusChange",
            EntityName = "Order",
            EntityId = order.Id.ToString(),
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus.ToString() }),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = dto.NewStatus.ToString(), Comment = dto.Comment }),
            CreatedAt = DateTime.UtcNow
        });

        // If transitioning to Cancelled from another status, restore inventory
        if (dto.NewStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
        {
            await RestoreOrderInventoryAsync(order, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetOrderByIdAsync(orderId, cancellationToken);
    }

    public async Task<Result> CancelOrderAsync(Guid orderId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
            return Result.Failure(Error.NotFound("Order", orderId));

        // If a customer is cancelling, ensure the order belongs to them
        if (userId.HasValue && order.UserId != userId.Value)
        {
            return Result.Failure(Error.Unauthorized("You are not authorized to cancel this order."));
        }

        // Enforce: Customers can cancel an order ONLY while status is Pending
        if (order.Status != OrderStatus.Pending)
        {
            return Result.Failure($"Orders in '{order.Status}' status cannot be cancelled by the customer. Only 'Pending' orders can be cancelled.");
        }

        var oldStatus = order.Status;
        order.Status = OrderStatus.Cancelled;

        var changedByName = _currentUserService.UserName ?? "Customer";
        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.Cancelled,
            ChangedByUserId = userId,
            ChangedByName = changedByName,
            Comment = reason ?? "Cancelled by customer. Stock restored.",
            CreatedAt = DateTime.UtcNow
        });

        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = changedByName,
            Action = "OrderCancel",
            EntityName = "Order",
            EntityId = order.Id.ToString(),
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus.ToString() }),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = OrderStatus.Cancelled.ToString(), Reason = reason }),
            CreatedAt = DateTime.UtcNow
        });

        await RestoreOrderInventoryAsync(order, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task RestoreOrderInventoryAsync(Order order, CancellationToken cancellationToken)
    {
        foreach (var item in order.Items)
        {
            if (item.ProductVariantId.HasValue)
            {
                var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.ProductVariantId.Value, cancellationToken);
                if (variant != null)
                {
                    variant.StockQuantity += item.Quantity;

                    _context.StockMovements.Add(new StockMovement
                    {
                        Id = Guid.NewGuid(),
                        ProductVariantId = variant.Id,
                        MovementType = StockMovementType.In,
                        Quantity = item.Quantity,
                        RemainingStock = variant.StockQuantity,
                        ReferenceNumber = order.OrderNumber,
                        Reason = "Order cancelled stock restoration",
                        CreatedByUserId = _currentUserService.UserId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }
    }

    private static OrderDetailDto MapToDetailDto(Order o) => new(
        o.Id,
        o.OrderNumber,
        o.UserId,
        $"{o.User.FirstName} {o.User.LastName}",
        o.User.Email ?? "",
        o.Status,
        o.SubTotal,
        o.ShippingCost,
        o.DiscountAmount,
        o.TotalAmount,
        o.CouponCodeSnapshot,
        o.ShippingFullName,
        o.ShippingPhoneNumber,
        o.ShippingAddressLine1,
        o.ShippingAddressLine2,
        o.ShippingCity,
        o.ShippingState,
        o.ShippingPostalCode,
        o.ShippingCountry,
        o.ShippingMethod?.Name,
        o.Notes,
        o.CreatedAt,
        o.Items.Select(i => new OrderItemDto(
            i.Id,
            i.ProductVariantId,
            i.ProductNameSnapshot,
            i.VariantNameSnapshot,
            i.SKUSnapshot,
            i.ProductImageUrlSnapshot,
            i.UnitPriceSnapshot,
            i.Quantity,
            i.TotalPrice)).ToList(),
        o.StatusHistory.OrderBy(sh => sh.CreatedAt).Select(sh => new OrderStatusHistoryDto(
            sh.Id,
            sh.OldStatus,
            sh.NewStatus,
            sh.ChangedByName,
            sh.Comment,
            sh.CreatedAt)).ToList(),
        o.Payment != null ? new PaymentSummaryDto(
            o.Payment.Id,
            o.Payment.TransactionId,
            o.Payment.Provider,
            o.Payment.Amount,
            o.Payment.Currency,
            o.Payment.Status,
            o.Payment.PaidAt) : null);
}
