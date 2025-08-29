using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConsoleFrontEnd.Models.Dtos;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services.Infrastructure;

/// <summary>
/// HTTP Response Handler that provides comprehensive error handling and response parsing
/// </summary>
public static class HttpResponseHelper
{
    /// <summary>
    /// Handles HTTP response with comprehensive error information from backend
    /// </summary>
    public static async Task<ApiResponseDto<T>> HandleHttpResponseAsync<T>(
        HttpResponseMessage response,
        ILogger logger,
        string operationName,
        T? fallbackData = default)
    {
        try
        {
            // Success response
            if (response.IsSuccessStatusCode)
            {
                // Use case-insensitive options to be robust across API casing differences
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var successResult = await response.Content.ReadFromJsonAsync<ApiResponseDto<T>>(jsonOptions);
                if (successResult != null)
                {
                    return successResult;
                }

                // Attempt a tolerant parse: read raw JSON and try to extract the "data" node then deserialize to T
                var raw = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    try
                    {
                        var generic = JsonSerializer.Deserialize<ApiResponseDto<JsonElement>>(raw, jsonOptions);
                        if (generic != null)
                        {
                            var dto = new ApiResponseDto<T>(generic.Message ?? $"{operationName} result")
                            {
                                RequestFailed = generic.RequestFailed,
                                ResponseCode = generic.ResponseCode,
                                Message = generic.Message,
                                TotalCount = generic.TotalCount
                            };

                            if (generic.Data.ValueKind != JsonValueKind.Null)
                            {
                                try
                                {
                                    dto.Data = JsonSerializer.Deserialize<T>(generic.Data.GetRawText(), jsonOptions);
                                }
                                catch (JsonException) { /* fall through to fallback below */ }
                            }

                            dto.Data ??= fallbackData;
                            dto.RequestFailed = false;
                            return dto;
                        }
                    }
                    catch (JsonException) { /* fall through to fallback below */ }
                }

                // Fallback if no structured response
                return new ApiResponseDto<T>($"{operationName} completed successfully")
                {
                    Data = fallbackData,
                    RequestFailed = false,
                    ResponseCode = response.StatusCode
                };
            }

            // Error response - try to get detailed error from backend
            var errorDetails = await ExtractErrorDetailsAsync(response, logger);

            logger.LogError("HTTP Error in {Operation}: {StatusCode} - {Message}",
                operationName, response.StatusCode, errorDetails.Message);

            return new ApiResponseDto<T>(errorDetails.Message)
            {
                Data = fallbackData,
                RequestFailed = true,
                ResponseCode = response.StatusCode
            };
        }
        catch (JsonException jsonEx)
        {
            var message = $"Invalid JSON response from server in {operationName}: {jsonEx.Message}";
            logger.LogError(jsonEx, message);
            return new ApiResponseDto<T>(message)
            {
                Data = fallbackData,
                RequestFailed = true,
                ResponseCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            var message = $"Unexpected error in {operationName}: {ex.Message}";
            logger.LogError(ex, message);
            return new ApiResponseDto<T>(message)
            {
                Data = fallbackData,
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError
            };
        }
    }

    /// <summary>
    /// Extracts detailed error information from HTTP response
    /// </summary>
    private static async Task<ErrorDetail> ExtractErrorDetailsAsync(HttpResponseMessage response, ILogger logger)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return new ErrorDetail
                {
                    Message = GetDefaultErrorMessage(response.StatusCode),
                    StatusCode = response.StatusCode
                };
            }

            // Try to parse as structured error response from backend
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponseDto<object>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.Message))
                {
                    return new ErrorDetail
                    {
                        Message = $"Server Error ({(int)response.StatusCode}): {errorResponse.Message}",
                        StatusCode = response.StatusCode,
                        Details = content
                    };
                }
            }
            catch (JsonException)
            {
                // Not a structured response, treat as plain text
            }

            // Try to parse as generic error object
            try
            {
                var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (errorObj != null)
                {
                    var errorMessage = ExtractErrorFromDictionary(errorObj);
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        return new ErrorDetail
                        {
                            Message = $"Server Error ({(int)response.StatusCode}): {errorMessage}",
                            StatusCode = response.StatusCode,
                            Details = content
                        };
                    }
                }
            }
            catch (JsonException)
            {
                // Not JSON, treat as plain text
            }

            // Fallback: use content as error message
            return new ErrorDetail
            {
                Message = $"Server Error ({(int)response.StatusCode}): {content.Substring(0, Math.Min(200, content.Length))}",
                StatusCode = response.StatusCode,
                Details = content
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract error details from response");
            return new ErrorDetail
            {
                Message = GetDefaultErrorMessage(response.StatusCode),
                StatusCode = response.StatusCode
            };
        }
    }

    /// <summary>
    /// Extracts error message from dictionary object
    /// </summary>
    private static string ExtractErrorFromDictionary(Dictionary<string, object> errorObj)
    {
        // Common error property names to check
        string[] errorKeys = ["error", "message", "title", "detail", "errorMessage", "description"];

        foreach (var key in errorKeys)
        {
            if (errorObj.TryGetValue(key, out var value) && value != null)
            {
                return value.ToString() ?? "";
            }
        }

        // If no standard error key found, try to get first string value
        var firstStringValue = errorObj.Values.FirstOrDefault(v => v is string)?.ToString();
        return firstStringValue ?? "";
    }

    /// <summary>
    /// Gets default error message for HTTP status codes
    /// </summary>
    private static string GetDefaultErrorMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Bad Request (400): The request was invalid or malformed.",
            HttpStatusCode.Unauthorized => "Unauthorized (401): Authentication is required.",
            HttpStatusCode.Forbidden => "Forbidden (403): Access is denied.",
            HttpStatusCode.NotFound => "Not Found (404): The requested resource was not found.",
            HttpStatusCode.MethodNotAllowed => "Method Not Allowed (405): The HTTP method is not supported.",
            HttpStatusCode.Conflict => "Conflict (409): There was a conflict with the current state.",
            HttpStatusCode.UnprocessableEntity => "Unprocessable Entity (422): The request was well-formed but contains semantic errors.",
            HttpStatusCode.InternalServerError => "Internal Server Error (500): An error occurred on the server.",
            HttpStatusCode.BadGateway => "Bad Gateway (502): Invalid response from upstream server.",
            HttpStatusCode.ServiceUnavailable => "Service Unavailable (503): The server is currently unavailable.",
            HttpStatusCode.GatewayTimeout => "Gateway Timeout (504): The server did not receive a timely response.",
            _ => $"HTTP Error ({(int)statusCode}): {statusCode}"
        };
    }

    /// <summary>
    /// Error detail container
    /// </summary>
    private class ErrorDetail
    {
        public string Message { get; set; } = "";
        public HttpStatusCode StatusCode { get; set; }
        public string? Details { get; set; }
    }
}
