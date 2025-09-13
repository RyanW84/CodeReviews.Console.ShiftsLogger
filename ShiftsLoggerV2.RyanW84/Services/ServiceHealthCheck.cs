using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ShiftsLoggerV2.RyanW84.Services;

/// <summary>
/// Health check service for monitoring application services
/// </summary>
public class ServiceHealthCheck : IHealthCheck
{
    private readonly ILogger<ServiceHealthCheck> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ServiceHealthCheck(
        ILogger<ServiceHealthCheck> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var healthChecks = new Dictionary<string, object>();
        var degradedReasons = new List<string>();
        var failureReasons = new List<string>();

        try
        {
            // Check database connectivity
            var dbHealth = await CheckDatabaseHealthAsync(cancellationToken);
            healthChecks["Database"] = dbHealth.Status.ToString();

            if (dbHealth.Status == HealthStatus.Unhealthy)
                failureReasons.Add("Database connection failed");
            else if (dbHealth.Status == HealthStatus.Degraded)
                degradedReasons.Add("Database performance degraded");

            // Check memory usage
            var memoryHealth = CheckMemoryHealth();
            healthChecks["Memory"] = memoryHealth.Status.ToString();

            if (memoryHealth.Status == HealthStatus.Degraded)
                degradedReasons.Add("High memory usage detected");

            // Check service responsiveness
            var serviceHealth = await CheckServiceHealthAsync(cancellationToken);
            healthChecks["Services"] = serviceHealth.Status.ToString();

            if (serviceHealth.Status == HealthStatus.Unhealthy)
                failureReasons.Add("Service responsiveness issues");
            else if (serviceHealth.Status == HealthStatus.Degraded)
                degradedReasons.Add("Service performance degraded");

            stopwatch.Stop();
            healthChecks["ResponseTime"] = $"{stopwatch.ElapsedMilliseconds}ms";

            // Determine overall health status
            if (failureReasons.Any())
            {
                return HealthCheckResult.Unhealthy(
                    $"Service health check failed: {string.Join(", ", failureReasons)}",
                    data: healthChecks);
            }
            else if (degradedReasons.Any())
            {
                return HealthCheckResult.Degraded(
                    $"Service health degraded: {string.Join(", ", degradedReasons)}",
                    data: healthChecks);
            }
            else
            {
                return HealthCheckResult.Healthy(
                    "All services are healthy",
                    data: healthChecks);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Health check failed with exception");

            return HealthCheckResult.Unhealthy(
                $"Health check failed: {ex.Message}",
                ex,
                new Dictionary<string, object>
                {
                    ["Exception"] = ex.Message,
                    ["ResponseTime"] = $"{stopwatch.ElapsedMilliseconds}ms"
                });
        }
    }

    private async Task<HealthCheckResult> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Try to resolve a repository to test database connectivity
            var repository = _serviceProvider.GetService(typeof(ShiftsLoggerV2.RyanW84.Repositories.Interfaces.IShiftRepository));
            if (repository == null)
            {
                return HealthCheckResult.Unhealthy("Database repository not available");
            }

            // Perform a simple database operation
            var testResult = await ((dynamic)repository).GetAllAsync(new ShiftsLoggerV2.RyanW84.Models.FilterOptions.ShiftFilterOptions(), cancellationToken);

            return testResult.IsSuccess
                ? HealthCheckResult.Healthy("Database connection successful")
                : HealthCheckResult.Unhealthy("Database query failed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}");
        }
    }

    private HealthCheckResult CheckMemoryHealth()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var memoryUsageMB = process.WorkingSet64 / 1024 / 1024;

            // Consider high memory usage as degraded (adjust threshold as needed)
            const long highMemoryThresholdMB = 500;

            if (memoryUsageMB > highMemoryThresholdMB)
            {
                return HealthCheckResult.Degraded(
                    $"High memory usage: {memoryUsageMB}MB",
                    data: new Dictionary<string, object>
                    {
                        ["MemoryUsageMB"] = memoryUsageMB,
                        ["ThresholdMB"] = highMemoryThresholdMB
                    });
            }

            return HealthCheckResult.Healthy(
                $"Memory usage normal: {memoryUsageMB}MB",
                data: new Dictionary<string, object>
                {
                    ["MemoryUsageMB"] = memoryUsageMB
                });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Memory health check failed");
            return HealthCheckResult.Unhealthy($"Memory health check failed: {ex.Message}");
        }
    }

    private async Task<HealthCheckResult> CheckServiceHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Test a simple service operation
            var service = _serviceProvider.GetService(typeof(ShiftsLoggerV2.RyanW84.Services.Interfaces.IShiftBusinessService));
            if (service == null)
            {
                return HealthCheckResult.Unhealthy("Business service not available");
            }

            // Perform a simple service operation
            var testResult = await ((dynamic)service).GetAllAsync(new ShiftsLoggerV2.RyanW84.Models.FilterOptions.ShiftFilterOptions(), cancellationToken);

            stopwatch.Stop();

            if (!testResult.IsSuccess)
            {
                return HealthCheckResult.Unhealthy(
                    "Service operation failed",
                    data: new Dictionary<string, object>
                    {
                        ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds
                    });
            }

            // Consider slow response as degraded (adjust threshold as needed)
            const int slowResponseThresholdMs = 5000;

            if (stopwatch.ElapsedMilliseconds > slowResponseThresholdMs)
            {
                return HealthCheckResult.Degraded(
                    $"Slow service response: {stopwatch.ElapsedMilliseconds}ms",
                    data: new Dictionary<string, object>
                    {
                        ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds,
                        ["ThresholdMs"] = slowResponseThresholdMs
                    });
            }

            return HealthCheckResult.Healthy(
                $"Service response time: {stopwatch.ElapsedMilliseconds}ms",
                data: new Dictionary<string, object>
                {
                    ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds
                });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Service health check failed");
            return HealthCheckResult.Unhealthy($"Service health check failed: {ex.Message}");
        }
    }
}
