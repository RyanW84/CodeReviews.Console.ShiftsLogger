using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ConsoleFrontEnd.Services.Common;

/// <summary>
/// Common service operations helper following SOLID principles and DRY
/// Provides reusable HTTP operations to reduce code duplication
/// </summary>
public class ApiServiceHelper
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public ApiServiceHelper(
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

    /// <summary>
    /// Generic GET all entities operation
    /// </summary>
    public async Task<ApiResponseDto<List<T>>> GetAllAsync<T>(string endpoint, string entityName)
    {
        try
        {
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.GetAsync(endpoint).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<T>>(
                response,
                _logger,
                $"Get All {entityName}",
                []
            ).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching {EntityName} from API", entityName);
            return CreateErrorResponse<List<T>>($"Network Error: {ex.Message}", []);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timeout while fetching {EntityName} from API", entityName);
            return CreateErrorResponse<List<T>>($"Request timed out while fetching {entityName}", []);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request was cancelled while fetching {EntityName} from API", entityName);
            return CreateErrorResponse<List<T>>($"Request was cancelled", []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching {EntityName} from API", entityName);
            return CreateErrorResponse<List<T>>($"Unexpected Error: {ex.Message}", []);
        }
    }

    /// <summary>
    /// Generic GET by ID operation
    /// </summary>
    public async Task<ApiResponseDto<T>> GetByIdAsync<T>(string endpoint, object id, string entityName)
    {
        try
        {
            var fullEndpoint = $"{endpoint}/{id}";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{fullEndpoint}");

            var response = await _httpClient.GetAsync(fullEndpoint).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<T>(
                response,
                _logger,
                $"Get {entityName} by ID"
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching {EntityName} {Id} from API", entityName, id);
            return CreateErrorResponse<T>($"Connection Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generic POST (Create) operation
    /// </summary>
    public async Task<ApiResponseDto<T>> CreateAsync<T>(string endpoint, T entity, string entityName)
    {
        try
        {
            _logger.LogInformation("Making POST request to: {RequestUrl}", $"{_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PostAsJsonAsync(endpoint, entity).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<T>(
                response,
                _logger,
                $"Create {entityName}"
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating {EntityName}", entityName);
            return CreateErrorResponse<T>($"Connection Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generic PUT (Update) operation
    /// </summary>
    public async Task<ApiResponseDto<T>> UpdateAsync<T>(string endpoint, object id, T entity, string entityName)
    {
        try
        {
            var fullEndpoint = $"{endpoint}/{id}";
            _logger.LogInformation("Making PUT request to: {RequestUrl}", $"{_httpClient.BaseAddress}{fullEndpoint}");

            var response = await _httpClient.PutAsJsonAsync(fullEndpoint, entity).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<T>(
                response,
                _logger,
                $"Update {entityName}"
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating {EntityName} {Id}", entityName, id);
            return CreateErrorResponse<T>($"Connection Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generic DELETE operation
    /// </summary>
    public async Task<ApiResponseDto<bool>> DeleteAsync(string endpoint, object id, string entityName)
    {
        try
        {
            var fullEndpoint = $"{endpoint}/{id}";
            _logger.LogInformation("Making DELETE request to: {RequestUrl}", $"{_httpClient.BaseAddress}{fullEndpoint}");

            var response = await _httpClient.DeleteAsync(fullEndpoint).ConfigureAwait(false);
            var result = await HttpResponseHelper.HandleHttpResponseAsync<bool>(
                response,
                _logger,
                $"Delete {entityName}"
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
            _logger.LogError(ex, "Error occurred while deleting {EntityName} {Id}", entityName, id);
            return CreateErrorResponse<bool>($"Connection Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generic local filtering operation
    /// </summary>
    public ApiResponseDto<List<T>> ApplyLocalFilter<T, TFilter>(
        ApiResponseDto<List<T>> allResponse,
        TFilter filter,
        Func<IQueryable<T>, TFilter, IQueryable<T>> applyFilters,
        string entityName)
    {
        try
        {
            if (allResponse.RequestFailed || allResponse.Data == null)
                return allResponse;

            var filtered = applyFilters(allResponse.Data.AsQueryable(), filter);
            var resultList = filtered.ToList();

            return new ApiResponseDto<List<T>>($"Filtered {entityName} successfully")
            {
                Data = resultList,
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                TotalCount = resultList.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering {EntityName}", entityName);
            return CreateErrorResponse<List<T>>($"Filter Error: {ex.Message}", []);
        }
    }

    /// <summary>
    /// Helper method to create consistent error responses
    /// </summary>
    private ApiResponseDto<T> CreateErrorResponse<T>(string message, T? fallbackData = default)
    {
        return new ApiResponseDto<T>(message)
        {
            Data = fallbackData,
            RequestFailed = true,
            ResponseCode = System.Net.HttpStatusCode.InternalServerError
        };
    }
}
