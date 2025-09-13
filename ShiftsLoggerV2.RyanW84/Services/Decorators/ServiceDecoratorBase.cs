using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Services.Decorators;

/// <summary>
/// Base decorator for service classes that adds cross-cutting concerns
/// </summary>
public abstract class ServiceDecoratorBase<TEntity, TFilter, TCreateDto, TUpdateDto>
    : IService<TEntity, TFilter, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity
{
    protected readonly IService<TEntity, TFilter, TCreateDto, TUpdateDto> _decoratedService;
    protected readonly ILogger _logger;

    protected ServiceDecoratorBase(
        IService<TEntity, TFilter, TCreateDto, TUpdateDto> decoratedService,
        ILogger logger)
    {
        _decoratedService = decoratedService ?? throw new ArgumentNullException(nameof(decoratedService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes an operation with performance monitoring and error handling
    /// </summary>
    protected async Task<T> ExecuteWithMonitoringAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting {OperationName} with args: {@Args}", operationName, args);

            var result = await operation();

            stopwatch.Stop();
            _logger.LogInformation(
                "Completed {OperationName} in {ElapsedMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Failed {OperationName} after {ElapsedMs}ms with error: {ErrorMessage}",
                operationName,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }

    /// <summary>
    /// Logs the result of an operation
    /// </summary>
    protected void LogResult<T>(Result<T> result, string operationName)
    {
        if (result.IsSuccess)
        {
            _logger.LogDebug("{OperationName} succeeded: {Message}", operationName, result.Message);
        }
        else
        {
            _logger.LogWarning("{OperationName} failed: {Message}", operationName, result.Message);
        }
    }

    public virtual async Task<Result<List<TEntity>>> GetAllAsync(TFilter filterOptions)
    {
        return await ExecuteWithMonitoringAsync(
            () => _decoratedService.GetAllAsync(filterOptions),
            nameof(GetAllAsync),
            filterOptions ?? (object)"null");
    }

    public virtual async Task<Result<TEntity>> GetByIdAsync(int id)
    {
        return await ExecuteWithMonitoringAsync(
            () => _decoratedService.GetByIdAsync(id),
            nameof(GetByIdAsync),
            id);
    }

    public virtual async Task<Result<TEntity>> CreateAsync(TCreateDto createDto)
    {
        return await ExecuteWithMonitoringAsync(
            () => _decoratedService.CreateAsync(createDto),
            nameof(CreateAsync),
            createDto ?? (object)"null");
    }

    public virtual async Task<Result<TEntity>> UpdateAsync(int id, TUpdateDto updateDto)
    {
        return await ExecuteWithMonitoringAsync(
            () => _decoratedService.UpdateAsync(id, updateDto),
            nameof(UpdateAsync),
            id,
            updateDto ?? (object)"null");
    }

    public virtual async Task<Result> DeleteAsync(int id)
    {
        return await ExecuteWithMonitoringAsync(
            () => _decoratedService.DeleteAsync(id),
            nameof(DeleteAsync),
            id);
    }
}
