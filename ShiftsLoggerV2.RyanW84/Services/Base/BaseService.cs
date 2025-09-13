using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Services.Base;

/// <summary>
/// Base service implementation that delegates to repository
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TFilter">Filter options type</typeparam>
/// <typeparam name="TCreateDto">Creation DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
public abstract class BaseService<TEntity, TFilter, TCreateDto, TUpdateDto>
    : IService<TEntity, TFilter, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity
{
    /// <summary>
    /// The repository instance used for data access operations
    /// </summary>
    protected readonly IRepository<TEntity, TFilter, TCreateDto, TUpdateDto> Repository;

    /// <summary>
    /// Initializes a new instance of the BaseService class
    /// </summary>
    /// <param name="repository">The repository to use for data operations</param>
    /// <exception cref="ArgumentNullException">Thrown when repository is null</exception>
    protected BaseService(IRepository<TEntity, TFilter, TCreateDto, TUpdateDto> repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Retrieves all entities matching the specified filter criteria
    /// </summary>
    /// <param name="filterOptions">The filter options to apply to the query</param>
    /// <returns>A result containing the list of entities or an error</returns>
    /// <example>
    /// <code>
    /// var filter = new MyFilterOptions { PageNumber = 1, PageSize = 10 };
    /// var result = await service.GetAllAsync(filter);
    /// if (result.IsSuccess)
    /// {
    ///     var entities = result.Data;
    ///     // Process entities
    /// }
    /// </code>
    /// </example>
    public virtual async Task<Result<List<TEntity>>> GetAllAsync(TFilter filterOptions)
    {
        // Add any business logic validation here if needed
        return await Repository.GetAllAsync(filterOptions).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a single entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <returns>A result containing the entity or an error if not found</returns>
    /// <exception cref="ArgumentException">Thrown when id is less than or equal to zero</exception>
    public virtual async Task<Result<TEntity>> GetByIdAsync(int id)
    {
        // Add any business logic validation here if needed
        if (id <= 0)
            return Result<TEntity>.Failure("ID must be greater than zero.");

        return await Repository.GetByIdAsync(id).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new entity based on the provided creation data
    /// </summary>
    /// <param name="createDto">The data transfer object containing creation information</param>
    /// <returns>A result containing the created entity or validation errors</returns>
    /// <remarks>
    /// This method performs business logic validation before creating the entity.
    /// Override <see cref="ValidateForCreateAsync(TCreateDto)"/> to add custom validation logic.
    /// </remarks>
    public virtual async Task<Result<TEntity>> CreateAsync(TCreateDto createDto)
    {
        // Add any business logic validation here if needed
        var validationResult = await ValidateForCreateAsync(createDto).ConfigureAwait(false);
        if (validationResult.IsFailure)
            return Result<TEntity>.Failure(validationResult.Message);

        return await Repository.CreateAsync(createDto).ConfigureAwait(false);
    }

    public virtual async Task<Result<TEntity>> UpdateAsync(int id, TUpdateDto updateDto)
    {
        // Add any business logic validation here if needed
        if (id <= 0)
            return Result<TEntity>.Failure("ID must be greater than zero.");

        var validationResult = await ValidateForUpdateAsync(id, updateDto).ConfigureAwait(false);
        if (validationResult.IsFailure)
            return Result<TEntity>.Failure(validationResult.Message);

        return await Repository.UpdateAsync(id, updateDto).ConfigureAwait(false);
    }

    public virtual async Task<Result> DeleteAsync(int id)
    {
        // Add any business logic validation here if needed
        if (id <= 0)
            return Result.Failure("ID must be greater than zero.");

        var validationResult = await ValidateForDeleteAsync(id).ConfigureAwait(false);
        if (validationResult.IsFailure)
            return Result.Failure(validationResult.Message);

        return await Repository.DeleteAsync(id).ConfigureAwait(false);
    }

    // Virtual methods for business logic validation - can be overridden by derived classes
    /// <summary>
    /// Validates business rules before creating an entity.
    /// Override this method in derived classes to add custom validation logic.
    /// </summary>
    /// <param name="createDto">The creation data to validate</param>
    /// <returns>A result indicating whether validation passed or failed</returns>
    /// <remarks>
    /// Return <see cref="Result.Success"/> if validation passes,
    /// or <see cref="Result.Failure(string)"/> with an error message if validation fails.
    /// </remarks>
    protected virtual ValueTask<Result> ValidateForCreateAsync(TCreateDto createDto)
    {
        return ValueTask.FromResult(Result.Success());
    }

    /// <summary>
    /// Validates business rules before updating an entity.
    /// Override this method in derived classes to add custom validation logic.
    /// </summary>
    /// <param name="id">The identifier of the entity being updated</param>
    /// <param name="updateDto">The update data to validate</param>
    /// <returns>A result indicating whether validation passed or failed</returns>
    protected virtual ValueTask<Result> ValidateForUpdateAsync(int id, TUpdateDto updateDto)
    {
        return ValueTask.FromResult(Result.Success());
    }

    /// <summary>
    /// Validates business rules before deleting an entity.
    /// Override this method in derived classes to add custom validation logic.
    /// </summary>
    /// <param name="id">The identifier of the entity being deleted</param>
    /// <returns>A result indicating whether validation passed or failed</returns>
    protected virtual async Task<Result> ValidateForDeleteAsync(int id)
    {
        await Task.CompletedTask; // Placeholder for async consistency
        return Result.Success();
    }
}
