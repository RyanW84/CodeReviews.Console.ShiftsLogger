using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Net.Http;
using System.Net.Http.Json;

namespace ConsoleFrontEnd.Services.Base;

/// <summary>
/// Base service implementing common API operations following SOLID principles
/// T - Entity type, TFilter - Filter options type, TKey - Primary key type
/// </summary>
public abstract class BaseApiService<T, TFilter, TKey> : IApiService<T, TFilter, TKey>
    where T : class
    where TFilter : class, new()
    where TKey : struct
{
    protected readonly HttpClient _httpClient;
    protected readonly IConfiguration _configuration;
    protected readonly ILogger _logger;

    protected BaseApiService(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration, 
        ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("ShiftsLoggerApi");
        _configuration = configuration;
        _logger = logger;
        
        // Set base address if not already set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7009");
        }
    }

    // Abstract properties that must be implemented by derived classes
    protected abstract string ApiEndpoint { get; }
    protected abstract string EntityName { get; }

    // Abstract methods for entity-specific operations
    protected abstract IQueryable<T> ApplyFilters(IQueryable<T> query, TFilter filter);
    protected abstract TKey GetEntityId(T entity);

    // Common CRUD operations with default implementations
    public virtual async Task<ApiResponseDto<List<T>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{ApiEndpoint}");

            var response = await _httpClient.GetAsync(ApiEndpoint).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<T>>(
                response,
                _logger,
                $"Get All {EntityName}",
                []
            ).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while fetching {EntityName} from API", EntityName);
            return CreateErrorResponse<List<T>>($"Connection Error: {ex.Message}", []);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timeout while fetching {EntityName} from API", EntityName);
            return CreateErrorResponse<List<T>>("Request timed out. Please try again.", []);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request was cancelled while fetching {EntityName} from API", EntityName);
            return CreateErrorResponse<List<T>>("Request was cancelled.", []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching {EntityName} from API", EntityName);
            return CreateErrorResponse<List<T>>($"Unexpected Error: {ex.Message}", []);
        }
    }

    public virtual async Task<ApiResponseDto<T>> GetByIdAsync(TKey id)
    {
        try
        {
            var endpoint = $"{ApiEndpoint}/{id}";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.GetAsync(endpoint).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<T>(
                response,
                _logger,
                $"Get {EntityName} by ID"
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching {EntityName} {Id} from API", EntityName, id);
            return CreateErrorResponse<T>($"Connection Error: {ex.Message}");
        }
    }

    public virtual async Task<ApiResponseDto<T>> CreateAsync(T entity)
    {
        try
        {
            _logger.LogInformation("Making POST request to: {RequestUrl}", $"{_httpClient.BaseAddress}{ApiEndpoint}");

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, entity).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<T>(
                response,
                _logger,
                $"Create {EntityName}"
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating {EntityName}", EntityName);
            return CreateErrorResponse<T>($"Connection Error: {ex.Message}");
        }
    }

    public virtual async Task<ApiResponseDto<T>> UpdateAsync(TKey id, T entity)
    {
        try
        {
            var endpoint = $"{ApiEndpoint}/{id}";
            _logger.LogInformation("Making PUT request to: {RequestUrl}", $"{_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PutAsJsonAsync(endpoint, entity).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<T>(
                response,
                _logger,
                $"Update {EntityName}"
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating {EntityName} {Id}", EntityName, id);
            return CreateErrorResponse<T>($"Connection Error: {ex.Message}");
        }
    }

    public virtual async Task<ApiResponseDto<bool>> DeleteAsync(TKey id)
    {
        try
        {
            var endpoint = $"{ApiEndpoint}/{id}";
            _logger.LogInformation("Making DELETE request to: {RequestUrl}", $"{_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.DeleteAsync(endpoint).ConfigureAwait(false);
            var result = await HttpResponseHelper.HandleHttpResponseAsync<bool>(
                response,
                _logger,
                $"Delete {EntityName}"
            ).ConfigureAwait(false);
            
            // If no specific response body, mark as successful if status is success
            if (response.IsSuccessStatusCode && !result.RequestFailed && !result.Data)
            {
                result.Data = true;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting {EntityName} {Id}", EntityName, id);
            return CreateErrorResponse<bool>($"Connection Error: {ex.Message}");
        }
    }

    public virtual async Task<ApiResponseDto<List<T>>> GetByFilterAsync(TFilter filter)
    {
        try
        {
            var allResponse = await GetAllAsync();
            if (allResponse.RequestFailed || allResponse.Data == null)
                return allResponse;

            var filtered = ApplyFilters(allResponse.Data.AsQueryable(), filter);
            var resultList = filtered.ToList();
            
            return new ApiResponseDto<List<T>>($"Filtered {EntityName} successfully")
            {
                Data = resultList,
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                TotalCount = resultList.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering {EntityName}", EntityName);
            return CreateErrorResponse<List<T>>($"Filter Error: {ex.Message}", []);
        }
    }

    // Helper method to create consistent error responses
    protected virtual ApiResponseDto<TResult> CreateErrorResponse<TResult>(string message, TResult? fallbackData = default)
    {
        return new ApiResponseDto<TResult>(message)
        {
            Data = fallbackData,
            RequestFailed = true,
            ResponseCode = System.Net.HttpStatusCode.InternalServerError
        };
    }

    // Dispose pattern
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
