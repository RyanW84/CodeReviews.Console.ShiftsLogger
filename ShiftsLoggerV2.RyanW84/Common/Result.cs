using System.Net;

namespace ShiftsLoggerV2.RyanW84.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string message, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        IsSuccess = isSuccess;
        Message = message;
        StatusCode = statusCode;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; }
    public HttpStatusCode StatusCode { get; }

    public static Result Success(string message = "Operation completed successfully", HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(true, message, statusCode);

    public static Result Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(false, message, statusCode);

    public static Result NotFound(string message = "Resource not found")
        => new(false, message, HttpStatusCode.NotFound);
}

/// <summary>
/// Represents the result of an operation that returns data
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>
public class Result<T> : Result
{
    protected Result(bool isSuccess, T? data, string message, HttpStatusCode statusCode = HttpStatusCode.OK)
        : base(isSuccess, message, statusCode)
    {
        Data = data;
    }

    public T? Data { get; }

    public static Result<T> Success(T data, string message = "Operation completed successfully", HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(true, data, message, statusCode);

    public static new Result<T> Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(false, default, message, statusCode);

    public static new Result<T> NotFound(string message = "Resource not found")
        => new(false, default, message, HttpStatusCode.NotFound);

    public static Result<T> Create(T? data, string message = "Resource created successfully")
        => new(true, data, message, HttpStatusCode.Created);
}
