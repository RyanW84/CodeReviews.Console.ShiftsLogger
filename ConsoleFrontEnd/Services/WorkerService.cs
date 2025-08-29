using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

public class WorkerService : IWorkerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WorkerService> _logger;

    public WorkerService(HttpClient httpClient, ILogger<WorkerService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponseDto<List<Worker>>> GetWorkersByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.WorkerFilterOptions filter, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var queryString = $"api/workers?" + BuildWorkerFilterQuery(filter, pageNumber, pageSize);
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Worker>>(
                response,
                _logger,
                "Get Workers By Filter",
                []
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering workers via API");
            return new ApiResponseDto<List<Worker>>($"Filter Error: {ex.Message}")
            {
                Data = [],
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    private string BuildWorkerFilterQuery(ConsoleFrontEnd.Models.FilterOptions.WorkerFilterOptions filter, int pageNumber = 1, int pageSize = 10)
    {
        List<string> query = [];
        if (filter.WorkerId.HasValue) query.Add($"WorkerId={filter.WorkerId.Value}");
        if (!string.IsNullOrWhiteSpace(filter.Name)) query.Add($"Name={Uri.EscapeDataString(filter.Name)}");
        if (!string.IsNullOrWhiteSpace(filter.Email)) query.Add($"Email={Uri.EscapeDataString(filter.Email)}");
        if (!string.IsNullOrWhiteSpace(filter.PhoneNumber)) query.Add($"PhoneNumber={Uri.EscapeDataString(filter.PhoneNumber)}");
        if (!string.IsNullOrWhiteSpace(filter.Search)) query.Add($"Search={Uri.EscapeDataString(filter.Search)}");

        // Add pagination parameters
        query.Add($"pageNumber={pageNumber}");
        query.Add($"pageSize={pageSize}");

        return string.Join("&", query);
    }

    public async Task<ApiResponseDto<List<Worker>>> GetAllWorkersAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var queryString = $"api/workers?pageNumber={pageNumber}&pageSize={pageSize}";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Worker>>(
                response,
                _logger,
                "Get All Workers",
                []
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching workers from API");
            return new ApiResponseDto<List<Worker>>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Data = [],
                RequestFailed = true
            };
        }
    }

    public async Task<ApiResponseDto<Worker?>> GetWorkerByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/workers/{id}").ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<Worker?>(
                response,
                _logger,
                $"Get Worker {id}",
                null
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching worker {WorkerId}", id);
            return new ApiResponseDto<Worker?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Worker?>> GetWorkerByNameAsync(string name)
    {
        try
        {
            // Use filter to find worker by name efficiently
            var filter = new ConsoleFrontEnd.Models.FilterOptions.WorkerFilterOptions { Name = name };
            var response = await GetWorkersByFilterAsync(filter);
            
            if (response.RequestFailed || response.Data == null)
                return new ApiResponseDto<Worker?>(response.Message ?? "Worker not found") 
                { 
                    RequestFailed = true, 
                    ResponseCode = response.ResponseCode,
                    Data = null 
                };

            var worker = response.Data.FirstOrDefault();
            return new ApiResponseDto<Worker?>(worker != null ? "Worker found" : "Worker not found")
            {
                Data = worker,
                RequestFailed = worker == null,
                ResponseCode = worker != null ? HttpStatusCode.OK : HttpStatusCode.NotFound
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching worker by name {WorkerName}", name);
            return new ApiResponseDto<Worker?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Worker>> CreateWorkerAsync(Worker worker)
    {
        var dto = new ConsoleFrontEnd.Models.Dtos.WorkerApiRequestDto
        {
            Name = worker.Name,
            Email = worker.Email ?? string.Empty,
            PhoneNumber = worker.PhoneNumber ?? string.Empty
        };
        var errors = Services.Validation.WorkerValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Worker>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/workers", dto).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<Worker>(
                response,
                _logger,
                "Create Worker",
                worker
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating worker");
            return new ApiResponseDto<Worker>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Worker?>> UpdateWorkerAsync(int id, Worker updatedWorker)
    {
        var dto = new ConsoleFrontEnd.Models.Dtos.WorkerApiRequestDto
        {
            Name = updatedWorker.Name,
            Email = updatedWorker.Email ?? string.Empty,
            PhoneNumber = updatedWorker.PhoneNumber ?? string.Empty
        };
        var errors = Services.Validation.WorkerValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Worker?>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workers/{id}", dto).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<Worker?>(
                response,
                _logger,
                $"Update Worker {id}",
                updatedWorker
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating worker {WorkerId}", id);
            return new ApiResponseDto<Worker?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteWorkerAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/workers/{id}").ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<bool>(
                response,
                _logger,
                $"Delete Worker {id}",
                false
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting worker {WorkerId}", id);
            return new ApiResponseDto<bool>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = false
            };
        }
    }
}
