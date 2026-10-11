using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductTag> ProductTags { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<ShippingMethod> ShippingMethods { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Review> Reviews { get; }
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<ReturnRequest> ReturnRequests { get; }
    DbSet<Banner> Banners { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<AppUser> Users { get; }
    DbSet<AppRole> Roles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
