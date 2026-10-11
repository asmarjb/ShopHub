using ECommerce.Application.Features.Categories.DTOs;
using ECommerce.Application.Features.Categories.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/categories")]
public class AdminCategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public AdminCategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("{id:guid}/delete-impact")]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> GetDeleteImpact(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetDeleteImpactAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        [FromQuery] Guid? reassignToCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.SoftDeleteAsync(id, reassignToCategoryId, cancellationToken);
        return HandleResult(result);
    }
}
