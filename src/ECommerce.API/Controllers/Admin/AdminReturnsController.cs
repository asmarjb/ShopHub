using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Returns.DTOs;
using ECommerce.Application.Features.Returns.Services;
using ECommerce.Domain.Constants;
using ECommerce.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Authorize]
[Route("api/admin/returns")]
public class AdminReturnsController : BaseApiController
{
    private readonly IReturnRequestService _returnRequestService;

    public AdminReturnsController(IReturnRequestService returnRequestService)
    {
        _returnRequestService = returnRequestService;
    }

    [HttpGet]
    [HasPermission(Permissions.ReturnManage)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _returnRequestService.GetAllAsync(pagination, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/process")]
    [HasPermission(Permissions.ReturnManage)]
    public async Task<IActionResult> ProcessReturn(Guid id, [FromBody] ProcessReturnRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _returnRequestService.ProcessAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }
}
