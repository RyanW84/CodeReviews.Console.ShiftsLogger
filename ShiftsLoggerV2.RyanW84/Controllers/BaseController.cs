using System.Net;
using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;

namespace ShiftsLoggerV2.RyanW84.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Handles Result and converts it to appropriate ActionResult<ApiResponseDto<string>>
    /// </summary>
    protected ActionResult<ApiResponseDto<string>> HandleNonGenericResult(Result result, string successMessage)
    {
        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, new ApiResponseDto<string>
            {
                RequestFailed = true,
                ResponseCode = result.StatusCode,
                Message = result.Message,
                Data = string.Empty,
                TotalCount = 0
            });
        }

        return Ok(new ApiResponseDto<string>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = successMessage,
            Data = string.Empty,
            TotalCount = 0
        });
    }

    /// <summary>
    /// Handles Result<T> and converts it to appropriate ActionResult<ApiResponseDto<T>>
    /// </summary>
    protected ActionResult<ApiResponseDto<T>> HandleResult<T>(Result<T> result, string successMessage)
    {
        if (!result.IsSuccess)
        {
            // Check if it's a NotFound result with empty data - treat as success with empty collection
            if (result.StatusCode == HttpStatusCode.NotFound && result.Data is IEnumerable<object> emptyEnumerable && !emptyEnumerable.Any())
            {
                return Ok(new ApiResponseDto<T>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    Message = successMessage,
                    Data = result.Data,
                    TotalCount = 0
                });
            }

            return StatusCode((int)result.StatusCode, new ApiResponseDto<T>
            {
                RequestFailed = true,
                ResponseCode = result.StatusCode,
                Message = result.Message,
                Data = default,
                TotalCount = 0
            });
        }

        return Ok(new ApiResponseDto<T>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = successMessage,
            Data = result.Data,
            TotalCount = result.Data is IEnumerable<object> enumerable ? enumerable.Count() : (result.Data != null ? 1 : 0)
        });
    }

    /// <summary>
    /// Handles Result<T> and converts it to appropriate ActionResult<PaginatedApiResponseDto<T>>
    /// </summary>
    protected ActionResult<PaginatedApiResponseDto<T>> HandlePaginatedResult<T>(Result<T> result, Models.FilterOptions.BaseFilterOptions filterOptions, string successMessage)
    {
        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, new PaginatedApiResponseDto<T>
            {
                RequestFailed = true,
                ResponseCode = result.StatusCode,
                Message = result.Message,
                Data = default,
                TotalCount = 0,
                PageNumber = filterOptions.PageNumber,
                PageSize = filterOptions.PageSize
            });
        }

        // Extract total count from the result message if available
        var totalCount = 0;
        if (result.Message.Contains("Total:"))
        {
            var messageParts = result.Message.Split("Total:");
            if (messageParts.Length > 1)
            {
                var totalPart = messageParts[1].Trim();
                // Extract just the number before any additional text
                var numberPart = totalPart.Split(' ').First();
                int.TryParse(numberPart, out totalCount);
            }
        }
        else if (result.Data is IEnumerable<object> enumerable)
        {
            // Fallback: if we can't extract total count, use the current page size as approximation
            totalCount = enumerable.Count();
        }

        return Ok(new PaginatedApiResponseDto<T>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = successMessage,
            Data = result.Data,
            TotalCount = totalCount,
            PageNumber = filterOptions.PageNumber,
            PageSize = filterOptions.PageSize
        });
    }

    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    protected ActionResult<ApiResponseDto<T>> Success<T>(T data, string message = "Operation completed successfully", int totalCount = 1)
    {
        return Ok(new ApiResponseDto<T>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = message,
            Data = data,
            TotalCount = data is IEnumerable<object> enumerable ? enumerable.Count() : totalCount
        });
    }

    /// <summary>
    /// Creates a standardized success response for operations without data
    /// </summary>
    protected ActionResult<ApiResponseDto<string>> Success(string message = "Operation completed successfully")
    {
        return Ok(new ApiResponseDto<string>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = message,
            Data = string.Empty,
            TotalCount = 0
        });
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    protected ActionResult<ApiResponseDto<T>> Error<T>(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return StatusCode((int)statusCode, new ApiResponseDto<T>
        {
            RequestFailed = true,
            ResponseCode = statusCode,
            Message = message,
            Data = default,
            TotalCount = 0
        });
    }

    /// <summary>
    /// Creates a standardized error response for operations without data
    /// </summary>
    protected ActionResult<ApiResponseDto<string>> Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return StatusCode((int)statusCode, new ApiResponseDto<string>
        {
            RequestFailed = true,
            ResponseCode = statusCode,
            Message = message,
            Data = string.Empty,
            TotalCount = 0
        });
    }

    /// <summary>
    /// Converts ModelState errors into a consistent ApiResponseDto shape
    /// </summary>
    protected ActionResult BadRequestModelState()
    {
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        var message = errors.Any() ? "Validation failed: " + string.Join("; ", errors) : "Validation failed";
        return BadRequest(new ApiResponseDto<object>
        {
            RequestFailed = true,
            ResponseCode = HttpStatusCode.BadRequest,
            Message = message,
            Data = null,
            TotalCount = 0
        });
    }
}
