using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Services.Decorators;

/// <summary>
/// Decorator that adds caching capabilities to services
/// </summary>
public class CachingServiceDecorator<TEntity, TFilter, TCreateDto, TUpdateDto>
    : ServiceDecoratorBase<TEntity, TFilter, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private readonly string _cacheKeyPrefix;

    public CachingServiceDecorator(
        IService<TEntity, TFilter, TCreateDto, TUpdateDto> decoratedService,
        IMemoryCache cache,
        ILogger logger,
        TimeSpan? cacheDuration = null,
        string? cacheKeyPrefix = null)
        : base(decoratedService, logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);
        _cacheKeyPrefix = cacheKeyPrefix ?? $"{typeof(TEntity).Name}_";
    }

    public override async Task<Result<List<TEntity>>> GetAllAsync(TFilter filterOptions)
    {
        var cacheKey = $"{_cacheKeyPrefix}GetAll_{filterOptions?.GetHashCode() ?? 0}";

        if (!_cache.TryGetValue(cacheKey, out Result<List<TEntity>>? cachedResult))
        {
            cachedResult = await base.GetAllAsync(filterOptions);

            if (cachedResult.IsSuccess)
            {
                _cache.Set(cacheKey, cachedResult, _cacheDuration);
                _logger.LogDebug("Cached result for {CacheKey}", cacheKey);
            }
        }
        else
        {
            _logger.LogDebug("Retrieved cached result for {CacheKey}", cacheKey);
        }

        return cachedResult!;
    }

    public override async Task<Result<TEntity>> GetByIdAsync(int id)
    {
        var cacheKey = $"{_cacheKeyPrefix}GetById_{id}";

        if (!_cache.TryGetValue(cacheKey, out Result<TEntity>? cachedResult))
        {
            cachedResult = await base.GetByIdAsync(id);

            if (cachedResult.IsSuccess)
            {
                _cache.Set(cacheKey, cachedResult, _cacheDuration);
                _logger.LogDebug("Cached result for {CacheKey}", cacheKey);
            }
        }
        else
        {
            _logger.LogDebug("Retrieved cached result for {CacheKey}", cacheKey);
        }

        return cachedResult!;
    }

    public override async Task<Result<TEntity>> CreateAsync(TCreateDto createDto)
    {
        var result = await base.CreateAsync(createDto);

        // Invalidate cache on successful creation
        if (result.IsSuccess)
        {
            InvalidateCache();
            _logger.LogDebug("Cache invalidated after successful creation");
        }

        return result;
    }

    public override async Task<Result<TEntity>> UpdateAsync(int id, TUpdateDto updateDto)
    {
        var result = await base.UpdateAsync(id, updateDto);

        // Invalidate cache on successful update
        if (result.IsSuccess)
        {
            InvalidateCache();
            _logger.LogDebug("Cache invalidated after successful update");
        }

        return result;
    }

    public override async Task<Result> DeleteAsync(int id)
    {
        var result = await base.DeleteAsync(id);

        // Invalidate cache on successful deletion
        if (result.IsSuccess)
        {
            InvalidateCache();
            _logger.LogDebug("Cache invalidated after successful deletion");
        }

        return result;
    }

    /// <summary>
    /// Invalidates all cache entries for this service
    /// </summary>
    private void InvalidateCache()
    {
        // Remove cache entries that start with our prefix
        var cacheEntries = _cache.GetType()
            .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .GetValue(_cache) as dynamic;

        if (cacheEntries != null)
        {
            foreach (var entry in cacheEntries)
            {
                var key = entry.Key as string;
                if (key != null && key.StartsWith(_cacheKeyPrefix))
                {
                    _cache.Remove(key);
                }
            }
        }
    }

    /// <summary>
    /// Manually invalidate cache for this service
    /// </summary>
    public void ClearCache()
    {
        InvalidateCache();
        _logger.LogInformation("Cache manually cleared for {ServiceType}", typeof(TEntity).Name);
    }
}
