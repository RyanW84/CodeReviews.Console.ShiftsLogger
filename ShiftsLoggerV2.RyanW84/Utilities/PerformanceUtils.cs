using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace ShiftsLoggerV2.RyanW84.Utilities;

/// <summary>
/// Utility class providing performance optimization helpers and monitoring tools
/// </summary>
public static class PerformanceUtils
{
    private static readonly ConcurrentDictionary<string, Stopwatch> _operationTimers = new();
    private static readonly ConcurrentDictionary<string, long> _operationCounts = new();

    /// <summary>
    /// Measures the execution time of an operation and logs performance metrics
    /// </summary>
    /// <typeparam name="T">The return type of the operation</typeparam>
    /// <param name="operation">The operation to measure</param>
    /// <param name="operationName">A descriptive name for the operation</param>
    /// <param name="logger">Optional logger for performance metrics</param>
    /// <returns>The result of the operation</returns>
    /// <example>
    /// <code>
    /// var result = await PerformanceUtils.MeasureAsync(
    ///     () => _repository.GetAllAsync(filter),
    ///     "GetAllEntities",
    ///     _logger);
    /// </code>
    /// </example>
    public static async Task<T> MeasureAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        ILogger? logger = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await operation();
            stopwatch.Stop();

            // Track operation count and average time
            _operationCounts.AddOrUpdate(operationName, 1, (_, count) => count + 1);

            logger?.LogDebug(
                "Operation '{OperationName}' completed in {ElapsedMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger?.LogWarning(
                ex,
                "Operation '{OperationName}' failed after {ElapsedMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Measures the execution time of a synchronous operation
    /// </summary>
    /// <typeparam name="T">The return type of the operation</typeparam>
    /// <param name="operation">The operation to measure</param>
    /// <param name="operationName">A descriptive name for the operation</param>
    /// <param name="logger">Optional logger for performance metrics</param>
    /// <returns>The result of the operation</returns>
    public static T Measure<T>(
        Func<T> operation,
        string operationName,
        ILogger? logger = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = operation();
            stopwatch.Stop();

            _operationCounts.AddOrUpdate(operationName, 1, (_, count) => count + 1);

            logger?.LogDebug(
                "Operation '{OperationName}' completed in {ElapsedMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger?.LogWarning(
                ex,
                "Operation '{OperationName}' failed after {ElapsedMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Gets performance statistics for all tracked operations
    /// </summary>
    /// <returns>A dictionary of operation names and their execution counts</returns>
    public static IReadOnlyDictionary<string, long> GetOperationStatistics()
    {
        return _operationCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Clears all performance statistics
    /// </summary>
    public static void ClearStatistics()
    {
        _operationCounts.Clear();
        _operationTimers.Clear();
    }

    /// <summary>
    /// Optimized string comparison for case-insensitive operations
    /// </summary>
    /// <param name="str1">First string to compare</param>
    /// <param name="str2">Second string to compare</param>
    /// <returns>True if strings are equal (case-insensitive), false otherwise</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(string? str1, string? str2)
    {
        if (str1 == null && str2 == null) return true;
        if (str1 == null || str2 == null) return false;

        return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Optimized null or whitespace check
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if the string is null, empty, or whitespace</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Optimized null or empty check
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if the string is null or empty</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Safely trims a string, returning empty string if null
    /// </summary>
    /// <param name="value">The string to trim</param>
    /// <returns>The trimmed string or empty string if null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SafeTrim(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Creates a hash code for multiple objects efficiently
    /// </summary>
    /// <param name="objects">The objects to hash</param>
    /// <returns>A hash code combining all objects</returns>
    public static int GetCombinedHashCode(params object?[] objects)
    {
        unchecked
        {
            int hash = 17;
            foreach (var obj in objects)
            {
                hash = hash * 31 + (obj?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }

    /// <summary>
    /// Memory-efficient way to check if a collection contains an item
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="collection">The collection to search</param>
    /// <param name="item">The item to find</param>
    /// <param name="comparer">Optional equality comparer</param>
    /// <returns>True if the item is found, false otherwise</returns>
    public static bool ContainsOptimized<T>(
        IEnumerable<T> collection,
        T item,
        IEqualityComparer<T>? comparer = null)
    {
        if (collection is IList<T> list)
        {
            // Use indexer for lists (O(1) access)
            for (int i = 0; i < list.Count; i++)
            {
                if (comparer?.Equals(list[i], item) ?? EqualityComparer<T>.Default.Equals(list[i], item))
                    return true;
            }
            return false;
        }
        else if (collection is ISet<T> set)
        {
            // Use set lookup (O(1) average case)
            return set.Contains(item);
        }
        else
        {
            // Fallback to LINQ Contains
            return comparer != null
                ? collection.Contains(item, comparer)
                : collection.Contains(item);
        }
    }

    /// <summary>
    /// Efficiently checks if any item in a collection matches a predicate
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="collection">The collection to search</param>
    /// <param name="predicate">The predicate to test</param>
    /// <returns>True if any item matches the predicate, false otherwise</returns>
    public static bool AnyOptimized<T>(IEnumerable<T> collection, Func<T, bool> predicate)
    {
        if (collection is IList<T> list)
        {
            // Use indexer for lists
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return true;
            }
            return false;
        }
        else
        {
            // Fallback to LINQ Any
            return collection.Any(predicate);
        }
    }

    /// <summary>
    /// Memory-efficient way to get distinct items from a collection
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="collection">The collection to process</param>
    /// <returns>An enumerable of distinct items</returns>
    public static IEnumerable<T> DistinctOptimized<T>(IEnumerable<T> collection)
    {
        if (collection is ISet<T>)
        {
            // Already a set, return as-is
            return collection;
        }

        // Use HashSet for efficient distinct operation
        return new HashSet<T>(collection);
    }
}
