using ShiftsLoggerV2.RyanW84.Common;

namespace ShiftsLoggerV2.RyanW84.Core.Interfaces;

/// <summary>
/// Base interface for entities with an integer ID
/// </summary>
public interface IEntity
{
    int Id { get; }
}

/// <summary>
/// Interface for read operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TFilter">Filter options type</typeparam>
public interface IReadRepository<TEntity, in TFilter> where TEntity : class, IEntity
{
    Task<Result<List<TEntity>>> GetAllAsync(TFilter filterOptions);
    Task<Result<TEntity>> GetByIdAsync(int id);
}

/// <summary>
/// Interface for write operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TCreateDto">Creation DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
public interface IWriteRepository<TEntity, in TCreateDto, in TUpdateDto> where TEntity : class, IEntity
{
    Task<Result<TEntity>> CreateAsync(TCreateDto createDto);
    Task<Result<TEntity>> UpdateAsync(int id, TUpdateDto updateDto);
    Task<Result> DeleteAsync(int id);
}

/// <summary>
/// Complete CRUD repository interface
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TFilter">Filter options type</typeparam>
/// <typeparam name="TCreateDto">Creation DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
public interface IRepository<TEntity, in TFilter, in TCreateDto, in TUpdateDto> 
    : IReadRepository<TEntity, TFilter>, IWriteRepository<TEntity, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity
{
}

/// <summary>
/// Interface for business logic services
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TFilter">Filter options type</typeparam>
/// <typeparam name="TCreateDto">Creation DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
public interface IService<TEntity, in TFilter, in TCreateDto, in TUpdateDto> 
    : IRepository<TEntity, TFilter, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity
{
}
