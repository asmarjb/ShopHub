namespace ECommerce.Application.Features.Statistics.DTOs;

public record RevenueChartPointDto(
    string Date,
    decimal Revenue,
    int OrderCount);

public record TopSellingProductDto(
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    int UnitsSold,
    decimal TotalRevenue);

public record OrderStatusCountDto(
    string Status,
    int Count);

public record AdminDashboardStatsDto(
    decimal TotalSales,
    int TotalOrders,
    int TotalUsers,
    int TotalProducts,
    int LowStockProductCount,
    List<OrderStatusCountDto> OrdersByStatus,
    List<RevenueChartPointDto> DailyRevenueLast30Days,
    List<TopSellingProductDto> TopSellingProducts);
