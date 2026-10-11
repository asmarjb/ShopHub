namespace ECommerce.Application.Common.Models;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified value is null.");

    public static Error NotFound(string entity, object key) =>
        new($"{entity}.NotFound", $"{entity} with identifier '{key}' was not found.");

    public static Error Validation(string message) =>
        new("Validation.Error", message);

    public static Error Unauthorized(string message = "You are not authorized to perform this action.") =>
        new("Auth.Unauthorized", message);

    public static Error Conflict(string message) =>
        new("Conflict.Error", message);

    public static Error BadRequest(string message) =>
        new("BadRequest.Error", message);
}
