using ECommerce.Application.Features.Categories.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetAllAsync(includeInactive, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryTreeAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetBySlugAsync(slug, cancellationToken);
        return HandleResult(result);
    }
}
