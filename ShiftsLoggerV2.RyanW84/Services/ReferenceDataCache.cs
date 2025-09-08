using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Models;

namespace ShiftsLoggerV2.RyanW84.Services;

/// <summary>
/// Enhanced service for caching reference data to improve performance
/// with advanced caching strategies and monitoring
/// </summary>
public class ReferenceDataCache
{
    private readonly IMemoryCache _cache;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReferenceDataCache> _logger;
    private const string WorkersCacheKey = "Workers";
    private const string LocationsCacheKey = "Locations";
    private const string WorkerByIdCacheKeyPrefix = "Worker_";
    private const string LocationByIdCacheKeyPrefix = "Location_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(2);

    // Cache statistics
    private long _cacheHits;
    private long _cacheMisses;

    public ReferenceDataCache(
        IMemoryCache cache,
        IServiceProvider serviceProvider,
        ILogger<ReferenceDataCache> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get cached workers or fetch from database if not cached
    /// </summary>
    public async Task<List<Worker>> GetWorkersAsync()
    {
        if (!_cache.TryGetValue(WorkersCacheKey, out List<Worker>? workers))
        {
            Interlocked.Increment(ref _cacheMisses);
            _logger.LogDebug("Cache miss for workers, fetching from database");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ShiftsLoggerDbContext>();

            workers = await dbContext.Workers
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetSlidingExpiration(SlidingExpiration)
                .RegisterPostEvictionCallback(OnCacheEviction);

            _cache.Set(WorkersCacheKey, workers, cacheOptions);
            _logger.LogDebug("Cached {Count} workers", workers.Count);
        }
        else
        {
            Interlocked.Increment(ref _cacheHits);
            _logger.LogTrace("Cache hit for workers");
        }

        return workers ?? new List<Worker>();
    }

    /// <summary>
    /// Get cached locations or fetch from database if not cached
    /// </summary>
    public async Task<List<Location>> GetLocationsAsync()
    {
        if (!_cache.TryGetValue(LocationsCacheKey, out List<Location>? locations))
        {
            Interlocked.Increment(ref _cacheMisses);
            _logger.LogDebug("Cache miss for locations, fetching from database");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ShiftsLoggerDbContext>();

            locations = await dbContext.Locations
                .AsNoTracking()
                .OrderBy(l => l.Name)
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetSlidingExpiration(SlidingExpiration)
                .RegisterPostEvictionCallback(OnCacheEviction);

            _cache.Set(LocationsCacheKey, locations, cacheOptions);
            _logger.LogDebug("Cached {Count} locations", locations.Count);
        }
        else
        {
            Interlocked.Increment(ref _cacheHits);
            _logger.LogTrace("Cache hit for locations");
        }

        return locations ?? new List<Location>();
    }

    /// <summary>
    /// Check if a worker exists using cached data
    /// </summary>
    public async Task<bool> WorkerExistsAsync(int workerId)
    {
        var workers = await GetWorkersAsync();
        return workers.Any(w => w.WorkerId == workerId);
    }

    /// <summary>
    /// Check if a location exists using cached data
    /// </summary>
    public async Task<bool> LocationExistsAsync(int locationId)
    {
        var locations = await GetLocationsAsync();
        return locations.Any(l => l.LocationId == locationId);
    }

    /// <summary>
    /// Get a worker by ID using cached data
    /// </summary>
    public async Task<Worker?> GetWorkerByIdAsync(int workerId)
    {
        var workers = await GetWorkersAsync();
        return workers.FirstOrDefault(w => w.WorkerId == workerId);
    }

    /// <summary>
    /// Get a location by ID using cached data
    /// </summary>
    public async Task<Location?> GetLocationByIdAsync(int locationId)
    {
        var locations = await GetLocationsAsync();
        return locations.FirstOrDefault(l => l.LocationId == locationId);
    }

    /// <summary>
    /// Invalidate the workers cache (call after worker modifications)
    /// </summary>
    public void InvalidateWorkersCache()
    {
        _cache.Remove(WorkersCacheKey);
        _logger.LogDebug("Workers cache invalidated");
    }

    /// <summary>
    /// Invalidate the locations cache (call after location modifications)
    /// </summary>
    public void InvalidateLocationsCache()
    {
        _cache.Remove(LocationsCacheKey);
        _logger.LogDebug("Locations cache invalidated");
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public (long Hits, long Misses, double HitRate) GetCacheStatistics()
    {
        var total = _cacheHits + _cacheMisses;
        var hitRate = total > 0 ? (double)_cacheHits / total : 0;
        return (_cacheHits, _cacheMisses, hitRate);
    }

    /// <summary>
    /// Clear all cache entries
    /// </summary>
    public void ClearAllCache()
    {
        InvalidateWorkersCache();
        InvalidateLocationsCache();
        _logger.LogInformation("All reference data cache cleared");
    }

    /// <summary>
    /// Callback method for cache eviction events
    /// </summary>
    private void OnCacheEviction(object key, object? value, EvictionReason reason, object? state)
    {
        _logger.LogDebug("Cache entry '{Key}' was evicted. Reason: {Reason}", key, reason);
    }
}
