using System.Reflection;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Features.AuditLogs.Services;
using ECommerce.Application.Features.Auth.Services;
using ECommerce.Application.Features.Banners.Services;
using ECommerce.Application.Features.Brands.Services;
using ECommerce.Application.Features.Cart.Services;
using ECommerce.Application.Features.Categories.Services;
using ECommerce.Application.Features.Coupons.Services;
using ECommerce.Application.Features.CsvExport;
using ECommerce.Application.Features.Orders.Services;
using ECommerce.Application.Features.Products.Services;
using ECommerce.Application.Features.Returns.Services;
using ECommerce.Application.Features.Reviews.Services;
using ECommerce.Application.Features.Roles.Services;
using ECommerce.Application.Features.Shipping.Services;
using ECommerce.Application.Features.Statistics.Services;
using ECommerce.Application.Features.StockMovements.Services;
using ECommerce.Application.Features.Tags.Services;
using ECommerce.Application.Features.Users.Services;
using ECommerce.Application.Features.Wishlist.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // AutoMapper
        services.AddAutoMapper(assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Application Services
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IShippingService, ShippingService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IReturnRequestService, ReturnRequestService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<IBannerService, BannerService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ICsvExportService, CsvExportService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITagService, TagService>();

        return services;
    }
}
