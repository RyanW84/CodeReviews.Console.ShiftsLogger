using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace ShiftsLoggerV2.RyanW84.Services;

/// <summary>
/// Service for collecting and exposing application metrics
/// </summary>
public class MetricsService : IDisposable
{
    private readonly Meter _meter;
    private readonly ILogger<MetricsService> _logger;

    // Counters
    private readonly Counter<long> _requestsTotal;
    private readonly Counter<long> _errorsTotal;
    private readonly Counter<long> _cacheHitsTotal;
    private readonly Counter<long> _cacheMissesTotal;

    // Histograms
    private readonly Histogram<double> _requestDuration;
    private readonly Histogram<double> _databaseQueryDuration;
    private readonly Histogram<long> _responseSize;

    // Gauges
    private readonly ObservableGauge<long> _activeConnections;
    private readonly ObservableGauge<long> _memoryUsage;
    private readonly ObservableGauge<long> _cacheSize;

    // Observable values
    private long _currentActiveConnections;
    private long _currentCacheSize;

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Create meter for this service
        _meter = new Meter("ShiftsLogger", "1.0.0");

        // Initialize counters
        _requestsTotal = _meter.CreateCounter<long>(
            "http_requests_total",
            description: "Total number of HTTP requests");

        _errorsTotal = _meter.CreateCounter<long>(
            "http_errors_total",
            description: "Total number of HTTP errors");

        _cacheHitsTotal = _meter.CreateCounter<long>(
            "cache_hits_total",
            description: "Total number of cache hits");

        _cacheMissesTotal = _meter.CreateCounter<long>(
            "cache_misses_total",
            description: "Total number of cache misses");

        // Initialize histograms
        _requestDuration = _meter.CreateHistogram<double>(
            "http_request_duration_seconds",
            description: "HTTP request duration in seconds");

        _databaseQueryDuration = _meter.CreateHistogram<double>(
            "database_query_duration_seconds",
            description: "Database query duration in seconds");

        _responseSize = _meter.CreateHistogram<long>(
            "http_response_size_bytes",
            description: "HTTP response size in bytes");

        // Initialize gauges
        _activeConnections = _meter.CreateObservableGauge<long>(
            "active_connections",
            () => _currentActiveConnections,
            description: "Number of active connections");

        _memoryUsage = _meter.CreateObservableGauge<long>(
            "memory_usage_bytes",
            GetMemoryUsage,
            description: "Current memory usage in bytes");

        _cacheSize = _meter.CreateObservableGauge<long>(
            "cache_size",
            () => _currentCacheSize,
            description: "Current cache size");

        _logger.LogInformation("Metrics service initialized");
    }

    /// <summary>
    /// Records an HTTP request
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="endpoint">Request endpoint</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="duration">Request duration in seconds</param>
    /// <param name="responseSize">Response size in bytes</param>
    public void RecordHttpRequest(
        string method,
        string endpoint,
        int statusCode,
        double duration,
        long responseSize = 0)
    {
        var tags = new TagList
        {
            { "method", method },
            { "endpoint", endpoint },
            { "status_code", statusCode.ToString() }
        };

        _requestsTotal.Add(1, tags);

        if (statusCode >= 400)
        {
            _errorsTotal.Add(1, tags);
        }

        _requestDuration.Record(duration, tags);

        if (responseSize > 0)
        {
            _responseSize.Record(responseSize, tags);
        }

        _logger.LogTrace(
            "Recorded HTTP request: {Method} {Endpoint} {StatusCode} in {Duration}s",
            method,
            endpoint,
            statusCode,
            duration);
    }

    /// <summary>
    /// Records a database query
    /// </summary>
    /// <param name="operation">Database operation type</param>
    /// <param name="table">Table name</param>
    /// <param name="duration">Query duration in seconds</param>
    /// <param name="rowCount">Number of rows affected/returned</param>
    public void RecordDatabaseQuery(
        string operation,
        string table,
        double duration,
        int rowCount = 0)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "table", table },
            { "row_count", rowCount.ToString() }
        };

        _databaseQueryDuration.Record(duration, tags);

        _logger.LogTrace(
            "Recorded database query: {Operation} on {Table} in {Duration}s ({RowCount} rows)",
            operation,
            table,
            duration,
            rowCount);
    }

    /// <summary>
    /// Records a cache operation
    /// </summary>
    /// <param name="operation">Cache operation (hit/miss)</param>
    /// <param name="cacheKey">Cache key</param>
    public void RecordCacheOperation(string operation, string cacheKey)
    {
        var tags = new TagList
        {
            { "cache_key", cacheKey }
        };

        if (operation.Equals("hit", StringComparison.OrdinalIgnoreCase))
        {
            _cacheHitsTotal.Add(1, tags);
        }
        else if (operation.Equals("miss", StringComparison.OrdinalIgnoreCase))
        {
            _cacheMissesTotal.Add(1, tags);
        }

        _logger.LogTrace("Recorded cache {Operation} for key: {CacheKey}", operation, cacheKey);
    }

    /// <summary>
    /// Updates the active connections count
    /// </summary>
    /// <param name="count">Current active connections</param>
    public void UpdateActiveConnections(long count)
    {
        _currentActiveConnections = count;
    }

    /// <summary>
    /// Updates the cache size
    /// </summary>
    /// <param name="size">Current cache size</param>
    public void UpdateCacheSize(long size)
    {
        _currentCacheSize = size;
    }

    /// <summary>
    /// Gets the current memory usage
    /// </summary>
    private long GetMemoryUsage()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            return process.WorkingSet64;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get memory usage");
            return 0;
        }
    }

    /// <summary>
    /// Creates a timer for measuring operation duration
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <returns>A disposable timer that records metrics when disposed</returns>
    public MetricsTimer StartTimer(string operationName)
    {
        return new MetricsTimer(operationName, this);
    }

    /// <summary>
    /// Disposable timer for measuring operation duration
    /// </summary>
    public class MetricsTimer : IDisposable
    {
        private readonly string _operationName;
        private readonly MetricsService _metricsService;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;

        public MetricsTimer(string operationName, MetricsService metricsService)
        {
            _operationName = operationName;
            _metricsService = metricsService;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _stopwatch.Stop();

            var duration = _stopwatch.Elapsed.TotalSeconds;

            // Record the duration (you can extend this to record different types of operations)
            _metricsService._requestDuration.Record(duration, new TagList
            {
                { "operation", _operationName }
            });
        }
    }

    public void Dispose()
    {
        _meter.Dispose();
        _logger.LogInformation("Metrics service disposed");
    }
}
