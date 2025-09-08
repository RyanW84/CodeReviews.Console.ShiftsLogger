namespace ConsoleFrontEnd.Services.Business;

/// <summary>
/// Represents the result of a business operation
/// Follows Command Pattern for consistent operation results
/// </summary>
public class OperationResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();

    public static OperationResult Success(string message = "Operation completed successfully")
        => new() { IsSuccess = true, Message = message };

    public static OperationResult Failure(string message, params string[] errors)
        => new() { IsSuccess = false, Message = message, Errors = errors.ToList() };

    public static OperationResult Failure(List<string> errors)
        => new() { IsSuccess = false, Message = "Operation failed", Errors = errors };
}

/// <summary>
/// Generic operation result with data
/// </summary>
public class OperationResult<T> : OperationResult
{
    public T? Data { get; init; }

    public static OperationResult<T> Success(T data, string message = "Operation completed successfully")
        => new() { IsSuccess = true, Data = data, Message = message };

    public static new OperationResult<T> Failure(string message, params string[] errors)
        => new() { IsSuccess = false, Message = message, Errors = errors.ToList() };

    public static new OperationResult<T> Failure(List<string> errors)
        => new() { IsSuccess = false, Message = "Operation failed", Errors = errors };
}
