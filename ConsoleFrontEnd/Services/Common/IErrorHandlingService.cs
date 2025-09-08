using System.Net;

namespace ConsoleFrontEnd.Services.Common;

/// <summary>
/// Centralized error handling service following Single Responsibility Principle
/// Provides consistent error mapping and user-friendly messages
/// </summary>
public interface IErrorHandlingService
{
    /// <summary>
    /// Maps HTTP status codes to user-friendly error messages
    /// </summary>
    string GetUserFriendlyErrorMessage(HttpStatusCode statusCode, string operation, string? apiMessage = null);

    /// <summary>
    /// Determines if an error should be retried
    /// </summary>
    bool ShouldRetry(HttpStatusCode statusCode);

    /// <summary>
    /// Gets error category for logging purposes
    /// </summary>
    ErrorCategory GetErrorCategory(HttpStatusCode statusCode);
}

public enum ErrorCategory
{
    ClientError,
    ServerError,
    NetworkError,
    ValidationError,
    AuthenticationError,
    AuthorizationError
}
