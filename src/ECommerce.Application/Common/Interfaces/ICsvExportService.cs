namespace ECommerce.Application.Common.Interfaces;

public interface ICsvExportService
{
    Task<byte[]> ExportProductsToCsvAsync(CancellationToken cancellationToken = default);
    Task<byte[]> ExportOrdersToCsvAsync(CancellationToken cancellationToken = default);
}
