using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Categories.Services;
using ECommerce.Application.Features.Products.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/trash")]
public class TrashController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public TrashController(
        IProductService productService,
        ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    // --- PRODUCTS IN TRASH ---

    [HttpGet("products")]
    [HasPermission(Permissions.TrashManage)]
    public async Task<IActionResult> GetDeletedProducts([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _productService.GetDeletedProductsAsync(pagination, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("products/{id:guid}/restore")]
    [HasPermission(Permissions.TrashManage)]
    public async Task<IActionResult> RestoreProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.RestoreAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("products/{id:guid}/hard-delete")]
    [HasPermission(Permissions.TrashManage)]
    public async Task<IActionResult> HardDeleteProduct(
        Guid id,
        [FromQuery] bool confirm = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.HardDeleteAsync(id, confirm, cancellationToken);
        return HandleResult(result);
    }

    // --- CATEGORIES IN TRASH ---

    [HttpGet("categories")]
    [HasPermission(Permissions.TrashManage)]
    public async Task<IActionResult> GetDeletedCategories(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetDeletedCategoriesAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("categories/{id:guid}/restore")]
    [HasPermission(Permissions.TrashManage)]
    public async Task<IActionResult> RestoreCategory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.RestoreAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("categories/{id:guid}/hard-delete")]
    [HasPermission(Permissions.TrashManage)]
    public async Task<IActionResult> HardDeleteCategory(
        Guid id,
        [FromQuery] bool confirm = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.HardDeleteAsync(id, confirm, cancellationToken);
        return HandleResult(result);
    }
}
