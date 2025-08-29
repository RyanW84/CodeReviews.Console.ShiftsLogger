using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ShiftsLoggerV2.RyanW84.HealthChecks;

/// <summary>
/// Custom health check to verify application-specific logic
/// </summary>
public class CustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Perform custom health checks
        // For example: check if critical services are available, memory usage, etc.

        var isHealthy = true;
        var description = "Application is healthy";

        // You can add custom logic here
        // For example:
        // - Check if external services are available
        // - Verify configuration is valid
        // - Check system resources

        if (isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy(description));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Application is not healthy"));
        }
    }
}
