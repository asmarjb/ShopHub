using System.Text;
using ECommerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.CsvExport;

public class CsvExportService : ICsvExportService
{
    private readonly IApplicationDbContext _context;

    public CsvExportService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportProductsToCsvAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,SKU,Category,Brand,BasePrice,CompareAtPrice,TotalStock,AverageRating,ReviewCount,IsActive,CreatedAt");

        foreach (var p in products)
        {
            var name = EscapeCsv(p.Name);
            var sku = EscapeCsv(p.SKU);
            var category = EscapeCsv(p.Category?.Name ?? "");
            var brand = EscapeCsv(p.Brand?.Name ?? "");
            var totalStock = p.Variants.Sum(v => v.StockQuantity);

            sb.AppendLine($"{p.Id},{name},{sku},{category},{brand},{p.BasePrice:F2},{p.CompareAtPrice:F2},{totalStock},{p.AverageRating:F1},{p.ReviewCount},{p.IsActive},{p.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportOrdersToCsvAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .Include(o => o.ShippingMethod)
            .Include(o => o.Payment)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Id,OrderNumber,CustomerName,Email,Status,ItemCount,SubTotal,ShippingCost,DiscountAmount,TotalAmount,PaymentStatus,PaymentProvider,CreatedAt");

        foreach (var o in orders)
        {
            var orderNum = EscapeCsv(o.OrderNumber);
            var customerName = EscapeCsv($"{o.User?.FirstName} {o.User?.LastName}");
            var email = EscapeCsv(o.User?.Email ?? "");
            var status = o.Status.ToString();
            var itemCount = o.Items.Sum(i => i.Quantity);
            var paymentStatus = o.Payment?.Status.ToString() ?? "None";
            var paymentProvider = o.Payment?.Provider.ToString() ?? "None";

            sb.AppendLine($"{o.Id},{orderNum},{customerName},{email},{status},{itemCount},{o.SubTotal:F2},{o.ShippingCost:F2},{o.DiscountAmount:F2},{o.TotalAmount:F2},{paymentStatus},{paymentProvider},{o.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
