using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services.Base;

/// <summary>
/// Generic interface for API service operations following Interface Segregation Principle
/// </summary>
public interface IApiService<T, TFilter, TKey> : IDisposable
    where T : class
    where TFilter : class
    where TKey : struct
{
    // Core CRUD operations
    Task<ApiResponseDto<List<T>>> GetAllAsync();
    Task<ApiResponseDto<T>> GetByIdAsync(TKey id);
    Task<ApiResponseDto<T>> CreateAsync(T entity);
    Task<ApiResponseDto<T>> UpdateAsync(TKey id, T entity);
    Task<ApiResponseDto<bool>> DeleteAsync(TKey id);
    
    // Query operations
    Task<ApiResponseDto<List<T>>> GetByFilterAsync(TFilter filter);
}
