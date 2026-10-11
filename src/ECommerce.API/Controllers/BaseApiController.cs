using ECommerce.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Data == null ? NoContent() : Ok(result.Data);
        }

        return HandleFailure(result);
    }

    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(new { success = true });
        }

        return HandleFailure(result);
    }

    private ActionResult HandleFailure(Result result)
    {
        if (result.ValidationErrors != null && result.ValidationErrors.Any())
        {
            return BadRequest(new
            {
                statusCode = 400,
                error = result.Error.Code,
                message = result.Error.Message,
                errors = result.ValidationErrors
            });
        }

        var code = result.Error.Code ?? string.Empty;

        if (code.EndsWith(".NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { statusCode = 404, error = result.Error.Code, message = result.Error.Message });
        }

        if (code.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new { statusCode = 401, error = result.Error.Code, message = result.Error.Message });
        }

        if (code.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { statusCode = 409, error = result.Error.Code, message = result.Error.Message });
        }

        return BadRequest(new { statusCode = 400, error = result.Error.Code, message = result.Error.Message });
    }
}
