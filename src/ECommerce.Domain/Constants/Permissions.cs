using System.Collections.ObjectModel;

namespace ECommerce.Domain.Constants;

public static class Permissions
{
    // Products
    public const string ProductView = "product.view";
    public const string ProductManage = "product.manage";

    // Categories
    public const string CategoryView = "category.view";
    public const string CategoryManage = "category.manage";

    // Orders
    public const string OrderView = "order.view";
    public const string OrderManage = "order.manage";

    // Users & Roles
    public const string UserManage = "user.manage";
    public const string RoleManage = "role.manage";

    // Statistics & Dashboard
    public const string StatisticsView = "statistics.view";

    // Trash / Recycle Bin
    public const string TrashManage = "trash.manage";

    // Coupons
    public const string CouponManage = "coupon.manage";

    // Reviews & Ratings
    public const string ReviewModerate = "review.moderate";

    // Audit Logs
    public const string AuditView = "audit.view";

    // Banners / CMS
    public const string BannerManage = "banner.manage";

    // Shipping Methods
    public const string ShippingManage = "shipping.manage";

    // Returns
    public const string ReturnManage = "return.manage";

    public static readonly IReadOnlyList<string> All = new ReadOnlyCollection<string>(
    [
        ProductView,
        ProductManage,
        CategoryView,
        CategoryManage,
        OrderView,
        OrderManage,
        UserManage,
        RoleManage,
        StatisticsView,
        TrashManage,
        CouponManage,
        ReviewModerate,
        AuditView,
        BannerManage,
        ShippingManage,
        ReturnManage
    ]);

    public static readonly IReadOnlyList<string> ManagerPermissions = new ReadOnlyCollection<string>(
    [
        ProductView,
        ProductManage,
        CategoryView,
        CategoryManage,
        OrderView,
        OrderManage,
        CouponManage,
        ReviewModerate,
        ReturnManage,
        ShippingManage,
        BannerManage
    ]);
}
