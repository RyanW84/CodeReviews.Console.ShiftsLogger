using System.Net;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;

namespace ShiftsLoggerV2.RyanW84.Factories;

/// <summary>
/// Factory for creating standardized API responses
/// </summary>
public static class ApiResponseFactory
{
    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponseDto<T> Success<T>(
        T data,
        string message = "Request completed successfully",
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponseDto<T>
        {
            RequestFailed = false,
            ResponseCode = statusCode,
            Message = message,
            Data = data,
            TotalCount = 1
        };
    }

    /// <summary>
    /// Creates a successful response with data and total count
    /// </summary>
    public static ApiResponseDto<T> Success<T>(
        T data,
        int totalCount,
        string message = "Request completed successfully",
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponseDto<T>
        {
            RequestFailed = false,
            ResponseCode = statusCode,
            Message = message,
            Data = data,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Creates a failure response
    /// </summary>
    public static ApiResponseDto<T> Failure<T>(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new ApiResponseDto<T>
        {
            RequestFailed = true,
            ResponseCode = statusCode,
            Message = message,
            Data = default,
            TotalCount = 0
        };
    }

    /// <summary>
    /// Creates a not found response
    /// </summary>
    public static ApiResponseDto<T> NotFound<T>(string resourceName = "Resource")
    {
        return Failure<T>($"{resourceName} not found", HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static ApiResponseDto<T> ValidationError<T>(string message)
    {
        return Failure<T>(message, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Creates an internal server error response
    /// </summary>
    public static ApiResponseDto<T> InternalServerError<T>(string message = "An internal server error occurred")
    {
        return Failure<T>(message, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Creates an unauthorized response
    /// </summary>
    public static ApiResponseDto<T> Unauthorized<T>(string message = "Unauthorized access")
    {
        return Failure<T>(message, HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Creates a forbidden response
    /// </summary>
    public static ApiResponseDto<T> Forbidden<T>(string message = "Access forbidden")
    {
        return Failure<T>(message, HttpStatusCode.Forbidden);
    }
}

/// <summary>
/// Factory for creating paginated API responses
/// </summary>
public static class PaginatedApiResponseFactory
{
    /// <summary>
    /// Creates a successful paginated response
    /// </summary>
    public static PaginatedApiResponseDto<T> Success<T>(
        T data,
        int pageNumber,
        int pageSize,
        int totalCount,
        string message = "Request completed successfully")
    {
        return new PaginatedApiResponseDto<T>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = message,
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
    }

    /// <summary>
    /// Creates a failure paginated response
    /// </summary>
    public static PaginatedApiResponseDto<T> Failure<T>(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new PaginatedApiResponseDto<T>
        {
            RequestFailed = true,
            ResponseCode = statusCode,
            Message = message,
            Data = default,
            TotalCount = 0,
            PageNumber = 0,
            PageSize = 0,
        };
    }

    /// <summary>
    /// Creates a not found paginated response
    /// </summary>
    public static PaginatedApiResponseDto<T> NotFound<T>(string resourceName = "Resources")
    {
        return Failure<T>($"{resourceName} not found", HttpStatusCode.NotFound);
    }
}

/// <summary>
/// Factory for creating API responses from business results
/// </summary>
public static class ResultApiResponseFactory
{
    /// <summary>
    /// Creates an API response from a business result
    /// </summary>
    public static ApiResponseDto<T> FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return ApiResponseFactory.Success(result.Data!, result.Message, result.StatusCode);
        }
        else
        {
            return ApiResponseFactory.Failure<T>(result.Message, result.StatusCode);
        }
    }

    /// <summary>
    /// Creates a paginated API response from a paginated business result
    /// </summary>
    public static PaginatedApiResponseDto<List<T>> FromPaginatedResult<T>(
        Result<List<T>> result,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        if (result.IsSuccess)
        {
            return PaginatedApiResponseFactory.Success(
                result.Data!,
                pageNumber,
                pageSize,
                totalCount,
                result.Message);
        }
        else
        {
            return PaginatedApiResponseFactory.Failure<List<T>>(result.Message, result.StatusCode);
        }
    }
}
