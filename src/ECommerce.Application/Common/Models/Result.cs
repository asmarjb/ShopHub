namespace ECommerce.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public IReadOnlyList<string> ValidationErrors { get; }

    protected Result(bool isSuccess, Error error, IEnumerable<string>? validationErrors = null)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Successful result cannot have an error.");

        if (!isSuccess && error == Error.None && (validationErrors == null || !validationErrors.Any()))
            throw new InvalidOperationException("Failed result must have an error or validation errors.");

        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string message) => new(false, Error.BadRequest(message));
    public static Result ValidationFailure(IEnumerable<string> errors) => new(false, Error.Validation("One or more validation failures occurred."), errors);

    public static Result<T> Success<T>(T data) => Result<T>.Success(data);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(string message) => Result<T>.Failure(Error.BadRequest(message));
    public static Result<T> ValidationFailure<T>(IEnumerable<string> errors) => Result<T>.ValidationFailure(errors);
}

public class Result<T> : Result
{
    private readonly T? _data;

    public T Data => IsSuccess
        ? _data!
        : throw new InvalidOperationException("Cannot access data of a failed result.");

    public T? DataOrDefault => _data;

    protected internal Result(T? data, bool isSuccess, Error error, IEnumerable<string>? validationErrors = null)
        : base(isSuccess, error, validationErrors)
    {
        _data = data;
    }

    public static Result<T> Success(T data) => new(data, true, Error.None);
    public new static Result<T> Failure(Error error) => new(default, false, error);
    public new static Result<T> Failure(string message) => new(default, false, Error.BadRequest(message));
    public new static Result<T> ValidationFailure(IEnumerable<string> errors) =>
        new(default, false, Error.Validation("One or more validation failures occurred."), errors);

    public static implicit operator Result<T>(T data) => Success(data);
}
