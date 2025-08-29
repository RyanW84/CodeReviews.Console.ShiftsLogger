using System;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace ShiftsLoggerV2.RyanW84.Common;

public static class ErrorMapper
{
    // Map exception types to friendly messages and HTTP status codes
    public static (HttpStatusCode Status, string Message) Map(Exception ex)
    {
        if (ex is ArgumentNullException) return (HttpStatusCode.BadRequest, "A required value was null.");
        if (ex is ArgumentException) return (HttpStatusCode.BadRequest, "Invalid argument provided.");
        if (ex is InvalidOperationException) return (HttpStatusCode.Conflict, "Invalid operation in current state.");
        if (ex is DbUpdateException) return (HttpStatusCode.Conflict, "Database update failed.");
        if (ex is TimeoutException) return (HttpStatusCode.RequestTimeout, "The operation timed out.");

        // Default fallback
        return (HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }
}
