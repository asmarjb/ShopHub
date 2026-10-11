using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Statistics.DTOs;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Statistics.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IApplicationDbContext _context;

    public StatisticsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AdminDashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalOrders = await _context.Orders.CountAsync(cancellationToken);
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalProducts = await _context.Products.CountAsync(cancellationToken);

        // Exclude cancelled orders from total revenue
        var totalSales = await _context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        var lowStockCount = await _context.ProductVariants
            .Where(v => v.StockQuantity <= v.LowStockThreshold)
            .Select(v => v.ProductId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Orders by Status
        var statusGroups = await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var ordersByStatus = Enum.GetValues<OrderStatus>()
            .Select(s => new OrderStatusCountDto(
                s.ToString(),
                statusGroups.FirstOrDefault(g => g.Status == s)?.Count ?? 0))
            .ToList();

        // Daily revenue for last 30 days
        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-29);
        var recentOrders = await _context.Orders
            .Where(o => o.CreatedAt >= thirtyDaysAgo && o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.CreatedAt, o.TotalAmount })
            .ToListAsync(cancellationToken);

        var dailyRevenue = new List<RevenueChartPointDto>();
        for (int i = 0; i < 30; i++)
        {
            var date = thirtyDaysAgo.AddDays(i);
            var dateOrders = recentOrders.Where(o => o.CreatedAt.Date == date).ToList();
            dailyRevenue.Add(new RevenueChartPointDto(
                date.ToString("yyyy-MM-dd"),
                dateOrders.Sum(o => o.TotalAmount),
                dateOrders.Count));
        }

        // Top 5 selling products
        var topItems = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled && oi.ProductVariantId.HasValue)
            .GroupBy(oi => oi.ProductVariantId)
            .Select(g => new
            {
                VariantId = g.Key,
                UnitsSold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.TotalPrice),
                Name = g.Select(x => x.ProductNameSnapshot).FirstOrDefault(),
                ImageUrl = g.Select(x => x.ProductImageUrlSnapshot).FirstOrDefault()
            })
            .OrderByDescending(x => x.UnitsSold)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topSellingProducts = topItems.Select(t => new TopSellingProductDto(
            t.VariantId ?? Guid.Empty,
            t.Name ?? "Product",
            t.ImageUrl,
            t.UnitsSold,
            t.TotalRevenue)).ToList();

        var stats = new AdminDashboardStatsDto(
            totalSales,
            totalOrders,
            totalUsers,
            totalProducts,
            lowStockCount,
            ordersByStatus,
            dailyRevenue,
            topSellingProducts);

        return Result.Success(stats);
    }
}
