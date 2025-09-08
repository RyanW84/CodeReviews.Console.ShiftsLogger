using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Repositories.Specifications;

namespace ShiftsLoggerV2.RyanW84.Repositories;

/// <summary>
/// Extension methods for applying specifications to Entity Framework queries
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Applies a specification to an IQueryable
    /// </summary>
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, ISpecification<T> spec)
        where T : class
    {
        // Apply criteria
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // Apply includes
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply string includes
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // Apply paging
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }

    /// <summary>
    /// Applies a specification and returns the first result or default
    /// </summary>
    public static async Task<T?> FirstOrDefaultAsync<T>(
        this IQueryable<T> query,
        ISpecification<T> spec,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return await query.ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Applies a specification and returns the first result
    /// </summary>
    public static async Task<T> FirstAsync<T>(
        this IQueryable<T> query,
        ISpecification<T> spec,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return await query.ApplySpecification(spec).FirstAsync(cancellationToken);
    }

    /// <summary>
    /// Applies a specification and returns a list
    /// </summary>
    public static async Task<List<T>> ToListAsync<T>(
        this IQueryable<T> query,
        ISpecification<T> spec,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return await query.ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Applies a specification and returns the count
    /// </summary>
    public static async Task<int> CountAsync<T>(
        this IQueryable<T> query,
        ISpecification<T> spec,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return await query.ApplySpecification(spec).CountAsync(cancellationToken);
    }

    /// <summary>
    /// Applies a specification and checks if any results exist
    /// </summary>
    public static async Task<bool> AnyAsync<T>(
        this IQueryable<T> query,
        ISpecification<T> spec,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return await query.ApplySpecification(spec).AnyAsync(cancellationToken);
    }
}
