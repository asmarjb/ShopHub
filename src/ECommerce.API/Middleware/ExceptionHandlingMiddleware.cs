using System.Net;
using System.Text.Json;
using FluentValidation;

namespace ECommerce.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate _next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        this._next = _next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        object response;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                response = new
                {
                    statusCode = (int)statusCode,
                    message = "Validation failed.",
                    errors
                };
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                response = new
                {
                    statusCode = (int)statusCode,
                    message = "Unauthorized access."
                };
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                response = new
                {
                    statusCode = (int)statusCode,
                    message = exception.Message
                };
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    statusCode = (int)statusCode,
                    message = exception.Message
                };
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response = new
                {
                    statusCode = (int)statusCode,
                    message = _env.IsDevelopment() ? exception.Message : "An unexpected server error occurred.",
                    detail = _env.IsDevelopment() ? exception.StackTrace : null
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(json);
    }
}
