using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Common.Interfaces;

public interface IFileStorage
{
    Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "uploads", CancellationToken cancellationToken = default);
    Task<Result> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
