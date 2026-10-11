using System.Linq.Expressions;
using System.Reflection;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Common;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply Global Query Filters dynamically to all ISoftDeletable entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyMethod = typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Public | BindingFlags.Static)!
                    .MakeGenericMethod(typeof(bool));
                var isDeletedProperty = Expression.Call(propertyMethod, parameter, Expression.Constant(nameof(ISoftDeletable.IsDeleted)));
                var compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);

                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserName = _currentUserService.UserName ?? "system";
        var auditEntries = OnBeforeSaveChanges(currentUserName);

        var result = await base.SaveChangesAsync(cancellationToken);

        await OnAfterSaveChangesAsync(auditEntries, cancellationToken);

        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges(string currentUserName)
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            // Handle audit timestamps and users
            if (entry.Entity is BaseAuditableEntity auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableEntity.CreatedAt = DateTime.UtcNow;
                    auditableEntity.CreatedBy = currentUserName;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditableEntity.UpdatedAt = DateTime.UtcNow;
                    auditableEntity.UpdatedBy = currentUserName;
                }
            }

            // Audit logging logic for critical administrative actions & mutations
            var entityName = entry.Entity.GetType().Name;
            var isAuditable = entry.Entity is ISoftDeletable
                           || entry.Entity is Order
                           || entry.Entity is RolePermission
                           || entry.Entity is AppRole
                           || entry.Entity is AppUser
                           || entry.Entity is Coupon;

            if (isAuditable)
            {
                var auditEntry = new AuditEntry(entry, _currentUserService)
                {
                    EntityName = entityName
                };

                // Determine specific action name
                if (entry.Entity is ISoftDeletable)
                {
                    var isDeletedProp = entry.Property(nameof(ISoftDeletable.IsDeleted));
                    if (isDeletedProp.IsModified)
                    {
                        var isDeleted = (bool)isDeletedProp.CurrentValue!;
                        auditEntry.Action = isDeleted ? "SoftDelete" : "Restore";

                        if (isDeleted)
                        {
                            entry.Property(nameof(ISoftDeletable.DeletedAt)).CurrentValue = DateTime.UtcNow;
                            entry.Property(nameof(ISoftDeletable.DeletedBy)).CurrentValue = currentUserName;
                        }
                        else
                        {
                            entry.Property(nameof(ISoftDeletable.DeletedAt)).CurrentValue = null;
                            entry.Property(nameof(ISoftDeletable.DeletedBy)).CurrentValue = null;
                        }
                    }
                }

                if (entry.State == EntityState.Deleted)
                {
                    auditEntry.Action = "HardDelete";
                }
                else if (entry.Entity is Order && entry.Property(nameof(Order.Status)).IsModified)
                {
                    auditEntry.Action = "OrderStatusChange";
                }
                else if (entry.Entity is RolePermission)
                {
                    auditEntry.Action = "PermissionChange";
                }
                else if (entry.Entity is AppRole && entry.State == EntityState.Modified)
                {
                    auditEntry.Action = "RoleChange";
                }

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    var propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.EntityId = property.CurrentValue?.ToString() ?? string.Empty;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                auditEntries.Add(auditEntry);
            }
        }

        return auditEntries;
    }

    private async Task OnAfterSaveChangesAsync(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            foreach (var prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.EntityId = prop.CurrentValue?.ToString() ?? string.Empty;
                }
                else
                {
                    auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        await base.SaveChangesAsync(cancellationToken);
    }
}
