using ECommerce.Application.Features.Tags.DTOs;
using ECommerce.Application.Features.Tags.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/tags")]
public class AdminTagsController : BaseApiController
{
    private readonly ITagService _tagService;

    public AdminTagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
    {
        var result = await _tagService.GetTagsAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> GetTagById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tagService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagDto dto, CancellationToken cancellationToken)
    {
        var result = await _tagService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.ProductManage)]
    public async Task<IActionResult> DeleteTag(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tagService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
