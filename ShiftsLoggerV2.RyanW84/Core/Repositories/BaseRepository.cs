using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;
using ShiftsLoggerV2.RyanW84.Data;
using System.Net;

namespace ShiftsLoggerV2.RyanW84.Core.Repositories;

/// <summary>
/// Base repository implementation with common CRUD operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TFilter">Filter options type</typeparam>
/// <typeparam name="TCreateDto">Creation DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
public abstract class BaseRepository<TEntity, TFilter, TCreateDto, TUpdateDto>
    : IRepository<TEntity, TFilter, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity
{
    protected readonly ShiftsLoggerDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    protected BaseRepository(ShiftsLoggerDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual async Task<Result<List<TEntity>>> GetAllAsync(TFilter filterOptions)
    {
        try
        {
            var query = BuildQuery(filterOptions);
            
            // Check if filter options support pagination
            if (filterOptions is Models.FilterOptions.BaseFilterOptions baseFilter)
            {
                baseFilter.ValidatePagination();
                
                // Get total count before pagination
                var totalCount = await query.CountAsync();
                
                // Apply pagination
                var paginatedQuery = query
                    .Skip((baseFilter.PageNumber - 1) * baseFilter.PageSize)
                    .Take(baseFilter.PageSize);
                
                var entities = await paginatedQuery.ToListAsync();

                if (!entities.Any() && totalCount > 0)
                {
                    return Result<List<TEntity>>.Success(
                        entities,
                        $"No {typeof(TEntity).Name.ToLower()}s found for page {baseFilter.PageNumber}. Total records: {totalCount}",
                        HttpStatusCode.OK
                    );
                }

                return Result<List<TEntity>>.Success(
                    entities,
                    $"{typeof(TEntity).Name}s retrieved successfully. Page {baseFilter.PageNumber} of {Math.Ceiling((double)totalCount / baseFilter.PageSize)}. Total: {totalCount}",
                    HttpStatusCode.OK
                );
            }
            else
            {
                // Fallback to non-paginated behavior for backward compatibility
                var entities = await query.ToListAsync();

                if (!entities.Any())
                {
                    return Result<List<TEntity>>.Success(
                        entities,
                        $"No {typeof(TEntity).Name.ToLower()}s found with the specified criteria.",
                        HttpStatusCode.OK
                    );
                }

                return Result<List<TEntity>>.Success(
                    entities,
                    $"{typeof(TEntity).Name}s retrieved successfully.",
                    HttpStatusCode.OK
                );
            }
        }
        catch (Exception ex)
        {
            return Result<List<TEntity>>.Failure(
                $"Error retrieving {typeof(TEntity).Name.ToLower()}s: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public virtual async Task<Result<TEntity>> GetByIdAsync(int id)
    {
        try
        {
            var entity = await GetEntityByIdAsync(id);

            if (entity == null)
            {
                return Result<TEntity>.NotFound($"{typeof(TEntity).Name} with ID {id} not found.");
            }

            return Result<TEntity>.Success(
                entity,
                $"{typeof(TEntity).Name} retrieved successfully.",
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return Result<TEntity>.Failure(
                $"Error retrieving {typeof(TEntity).Name.ToLower()}: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public virtual async Task<Result<TEntity>> CreateAsync(TCreateDto createDto)
    {
        try
        {
            var entity = await CreateEntityFromDtoAsync(createDto);
            await DbSet.AddAsync(entity);
            await DbContext.SaveChangesAsync();

            try
            {
                // Log created entity id for debugging (IEntity.Id is implemented by entities)
                Console.WriteLine($"Created {typeof(TEntity).Name} with ID {entity.Id}");
            }
            catch
            {
                // ignore any logging failures
            }

            return Result<TEntity>.Create(
                entity,
                $"{typeof(TEntity).Name} created successfully."
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create {typeof(TEntity).Name}: {ex.Message}\n{ex}");
            return Result<TEntity>.Failure(
                $"Error creating {typeof(TEntity).Name.ToLower()}: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public virtual async Task<Result<TEntity>> UpdateAsync(int id, TUpdateDto updateDto)
    {
        try
        {
            var entity = await GetEntityByIdAsync(id);

            if (entity == null)
            {
                return Result<TEntity>.NotFound($"{typeof(TEntity).Name} with ID {id} not found.");
            }

            await UpdateEntityFromDtoAsync(entity, updateDto);
            DbSet.Update(entity);
            await DbContext.SaveChangesAsync();

            // Reload the entity via GetEntityByIdAsync so any navigation properties (Includes) are populated
            var reloaded = await GetEntityByIdAsync(id);
            if (reloaded == null)
            {
                return Result<TEntity>.Failure($"{typeof(TEntity).Name} updated but failed to reload.", HttpStatusCode.InternalServerError);
            }

            return Result<TEntity>.Success(
                reloaded,
                $"{typeof(TEntity).Name} updated successfully.",
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return Result<TEntity>.Failure(
                $"Error updating {typeof(TEntity).Name.ToLower()}: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public virtual async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var entity = await GetEntityByIdAsync(id);

            if (entity == null)
            {
                return Result.NotFound($"{typeof(TEntity).Name} with ID {id} not found.");
            }

            DbSet.Remove(entity);
            await DbContext.SaveChangesAsync();

            return Result.Success(
                $"{typeof(TEntity).Name} deleted successfully.",
                HttpStatusCode.NoContent
            );
        }
        catch (Exception ex)
        {
            return Result.Failure(
                $"Error deleting {typeof(TEntity).Name.ToLower()}: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    // Abstract methods to be implemented by derived classes
    protected abstract IQueryable<TEntity> BuildQuery(TFilter filterOptions);
    protected abstract Task<TEntity?> GetEntityByIdAsync(int id);
    protected abstract Task<TEntity> CreateEntityFromDtoAsync(TCreateDto createDto);
    protected abstract Task UpdateEntityFromDtoAsync(TEntity entity, TUpdateDto updateDto);
}
