using ECommerce.Application.Features.Returns.DTOs;
using ECommerce.Application.Features.Returns.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
[Route("api/returns")]
public class ReturnsController : BaseApiController
{
    private readonly IReturnRequestService _returnRequestService;

    public ReturnsController(IReturnRequestService returnRequestService)
    {
        _returnRequestService = returnRequestService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _returnRequestService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("my-returns")]
    public async Task<IActionResult> GetMyReturns(CancellationToken cancellationToken)
    {
        var result = await _returnRequestService.GetUserReturnRequestsAsync(cancellationToken);
        return HandleResult(result);
    }
}
