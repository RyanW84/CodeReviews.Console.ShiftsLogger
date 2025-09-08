using System.Net;
using ConsoleFrontEnd.Services.Common;

namespace ConsoleFrontEnd.Services.Infrastructure;

/// <summary>
/// Implementation of centralized error handling
/// Follows Open/Closed Principle - extensible for new error types
/// </summary>
public class ErrorHandlingService : IErrorHandlingService
{
    private readonly Dictionary<HttpStatusCode, (string Message, ErrorCategory Category)> _errorMappings;

    public ErrorHandlingService()
    {
        _errorMappings = new Dictionary<HttpStatusCode, (string, ErrorCategory)>
        {
            [HttpStatusCode.BadRequest] = ("Invalid request. Please check your input.", ErrorCategory.ValidationError),
            [HttpStatusCode.Unauthorized] = ("You are not authorized to perform this action.", ErrorCategory.AuthenticationError),
            [HttpStatusCode.Forbidden] = ("Access denied. You don't have permission for this operation.", ErrorCategory.AuthorizationError),
            [HttpStatusCode.NotFound] = ("The requested resource was not found.", ErrorCategory.ClientError),
            [HttpStatusCode.Conflict] = ("The operation conflicts with the current state.", ErrorCategory.ValidationError),
            [HttpStatusCode.RequestTimeout] = ("The request timed out. Please try again.", ErrorCategory.NetworkError),
            [HttpStatusCode.UnprocessableEntity] = ("The request data is invalid.", ErrorCategory.ValidationError),
            [HttpStatusCode.InternalServerError] = ("An internal server error occurred. Please try again later.", ErrorCategory.ServerError),
            [HttpStatusCode.BadGateway] = ("Service temporarily unavailable. Please try again later.", ErrorCategory.ServerError),
            [HttpStatusCode.ServiceUnavailable] = ("Service is currently unavailable. Please try again later.", ErrorCategory.ServerError),
            [HttpStatusCode.GatewayTimeout] = ("Service request timed out. Please try again later.", ErrorCategory.NetworkError)
        };
    }

    public string GetUserFriendlyErrorMessage(HttpStatusCode statusCode, string operation, string? apiMessage = null)
    {
        var baseMessage = _errorMappings.TryGetValue(statusCode, out var mapping)
            ? mapping.Message
            : "An unexpected error occurred.";

        var fullMessage = $"Failed to {operation.ToLower()}: {baseMessage}";

        if (!string.IsNullOrWhiteSpace(apiMessage))
        {
            fullMessage += $" Details: {apiMessage}";
        }

        return fullMessage;
    }

    public bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.RequestTimeout => true,
            HttpStatusCode.InternalServerError => true,
            HttpStatusCode.BadGateway => true,
            HttpStatusCode.ServiceUnavailable => true,
            HttpStatusCode.GatewayTimeout => true,
            _ => false
        };
    }

    public ErrorCategory GetErrorCategory(HttpStatusCode statusCode)
    {
        return _errorMappings.TryGetValue(statusCode, out var mapping)
            ? mapping.Category
            : ErrorCategory.ClientError;
    }
}
