using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Models;

namespace ShiftsLoggerV2.RyanW84.Services;

/// <summary>
/// Service for caching reference data to improve performance
/// </summary>
public class ReferenceDataCache
{
    private readonly IMemoryCache _cache;
    private readonly IServiceProvider _serviceProvider;
    private const string WorkersCacheKey = "Workers";
    private const string LocationsCacheKey = "Locations";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ReferenceDataCache(IMemoryCache cache, IServiceProvider serviceProvider)
    {
        _cache = cache;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Get cached workers or fetch from database if not cached
    /// </summary>
    public async Task<List<Worker>> GetWorkersAsync()
    {
        if (!_cache.TryGetValue(WorkersCacheKey, out List<Worker>? workers))
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Data.ShiftsLoggerDbContext>();

            workers = await dbContext.Workers
                .AsNoTracking()
                .ToListAsync();

            _cache.Set(WorkersCacheKey, workers, CacheDuration);
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
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Data.ShiftsLoggerDbContext>();

            locations = await dbContext.Locations
                .AsNoTracking()
                .ToListAsync();

            _cache.Set(LocationsCacheKey, locations, CacheDuration);
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
    }

    /// <summary>
    /// Invalidate the locations cache (call after location modifications)
    /// </summary>
    public void InvalidateLocationsCache()
    {
        _cache.Remove(LocationsCacheKey);
    }
}
