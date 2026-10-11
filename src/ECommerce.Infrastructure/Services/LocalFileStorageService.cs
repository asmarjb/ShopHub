using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class LocalFileStorageService : IFileStorage
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<Result<string>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder = "uploads",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var targetFolder = Path.Combine(webRootPath, folder);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            await using var outputStream = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(outputStream, cancellationToken);

            var relativeUrl = $"/{folder}/{uniqueFileName}".Replace('\\', '/');
            return Result.Success(relativeUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
            return Result.Failure<string>($"File upload failed: {ex.Message}");
        }
    }

    public Task<Result> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.FromResult(Result.Success());

            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FileUrl}", fileUrl);
            return Task.FromResult(Result.Failure($"File deletion failed: {ex.Message}"));
        }
    }
}
