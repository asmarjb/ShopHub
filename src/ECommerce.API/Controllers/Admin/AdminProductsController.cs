using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Features.Products.DTOs;
using ECommerce.Application.Features.Products.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/products")]
public class AdminProductsController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly IFileStorage _fileStorage;
    private readonly ICsvExportService _csvExportService;

    public AdminProductsController(
        IProductService productService,
        IFileStorage fileStorage,
        ICsvExportService csvExportService)
    {
        _productService = productService;
        _fileStorage = fileStorage;
        _csvExportService = csvExportService;
    }

    [HttpPost]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.SoftDeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/variants")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> AddVariant(Guid id, [FromBody] CreateProductVariantDto dto, CancellationToken cancellationToken)
    {
        var result = await _productService.AddVariantAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("variants/{variantId:guid}")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> DeleteVariant(Guid variantId, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteVariantAsync(variantId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/images")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> UploadImages(
        Guid id,
        [FromForm] IFormFileCollection files,
        [FromForm] int? primaryIndex,
        [FromForm] string? altText,
        CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });

        var uploadedImages = new List<ProductImageDto>();

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            if (file.Length == 0) continue;

            await using var stream = file.OpenReadStream();
            var uploadResult = await _fileStorage.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                "uploads/products",
                cancellationToken);

            if (!uploadResult.IsSuccess)
                return HandleResult(uploadResult);

            var isPrimary = primaryIndex.HasValue ? (primaryIndex.Value == i) : (i == 0);
            var addImageResult = await _productService.AddImageAsync(
                id,
                uploadResult.Data,
                altText,
                isPrimary,
                cancellationToken);

            if (!addImageResult.IsSuccess)
                return HandleResult(addImageResult);

            uploadedImages.Add(addImageResult.Data);
        }

        return Ok(uploadedImages);
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteImageAsync(id, imageId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/images/{imageId:guid}/set-primary")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> SetPrimaryImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await _productService.SetPrimaryImageAsync(id, imageId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("export-csv")]
    [HasPermission(Permissions.ProductView)]
    public async Task<IActionResult> ExportProductsCsv(CancellationToken cancellationToken)
    {
        var bytes = await _csvExportService.ExportProductsToCsvAsync(cancellationToken);
        var fileName = $"products-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
