using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

public class ShiftService : IShiftService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(HttpClient httpClient, ILogger<ShiftService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponseDto<List<Shift>>> GetShiftsByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var queryString = $"api/shifts?" + BuildShiftFilterQuery(filter, pageNumber, pageSize);
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Shift>>(
                response,
                _logger,
                "Get Shifts by Filter",
                []
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering shifts via API");
            return new ApiResponseDto<List<Shift>>($"Filter Error: {ex.Message}")
            {
                Data = [],
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    private string BuildShiftFilterQuery(ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter, int pageNumber = 1, int pageSize = 10)
    {
        List<string> query = [];
        if (filter.ShiftId.HasValue) query.Add($"ShiftId={filter.ShiftId.Value}");
        if (filter.WorkerId.HasValue) query.Add($"WorkerId={filter.WorkerId.Value}");
        if (filter.LocationId.HasValue) query.Add($"LocationId={filter.LocationId.Value}");
        if (filter.StartTime.HasValue) query.Add($"StartTime={Uri.EscapeDataString(filter.StartTime.Value.ToString("o"))}");
        if (filter.EndTime.HasValue) query.Add($"EndTime={Uri.EscapeDataString(filter.EndTime.Value.ToString("o"))}");
        if (!string.IsNullOrWhiteSpace(filter.LocationName)) query.Add($"LocationName={Uri.EscapeDataString(filter.LocationName)}");
        if (!string.IsNullOrWhiteSpace(filter.WorkerName)) query.Add($"WorkerName={Uri.EscapeDataString(filter.WorkerName)}");
        if (filter.MinDurationMinutes.HasValue) query.Add($"MinDurationMinutes={filter.MinDurationMinutes.Value}");
        if (filter.MaxDurationMinutes.HasValue) query.Add($"MaxDurationMinutes={filter.MaxDurationMinutes.Value}");

        // Add pagination parameters
        query.Add($"pageNumber={pageNumber}");
        query.Add($"pageSize={pageSize}");

        return string.Join("&", query);
    }

    private static ApiResponseDto<List<Shift>> FilterShiftsLocally(ApiResponseDto<List<Shift>> allResponse, ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter)
    {
        var filtered = (allResponse.Data ?? []).AsQueryable();

        if (filter.ShiftId.HasValue)
            filtered = filtered.Where(s => s.ShiftId == filter.ShiftId.Value);
        if (filter.WorkerId.HasValue)
            filtered = filtered.Where(s => s.WorkerId == filter.WorkerId.Value);
        if (filter.LocationId.HasValue)
            filtered = filtered.Where(s => s.LocationId == filter.LocationId.Value);
        if (filter.StartTime.HasValue)
            filtered = filtered.Where(s => s.StartTime >= filter.StartTime.Value);
        if (filter.EndTime.HasValue)
            filtered = filtered.Where(s => s.EndTime <= filter.EndTime.Value);
        if (!string.IsNullOrWhiteSpace(filter.LocationName))
            filtered = filtered.Where(s => s.Location != null && s.Location.Name.Contains(filter.LocationName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(filter.WorkerName))
            filtered = filtered.Where(s => s.Worker != null && s.Worker.Name.Contains(filter.WorkerName, StringComparison.OrdinalIgnoreCase));
        if (filter.MinDurationMinutes.HasValue)
            filtered = filtered.Where(s => s.Duration.TotalMinutes >= filter.MinDurationMinutes.Value);
        if (filter.MaxDurationMinutes.HasValue)
            filtered = filtered.Where(s => s.Duration.TotalMinutes <= filter.MaxDurationMinutes.Value);

        var resultList = filtered.ToList();
        return new ApiResponseDto<List<Shift>>("Filtered shifts successfully")
        {
            Data = resultList,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            TotalCount = resultList.Count
        };
    }

    public async Task<ApiResponseDto<List<Shift>>> GetAllShiftsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var queryString = $"api/shifts?pageNumber={pageNumber}&pageSize={pageSize}";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Shift>>(
                response,
                _logger,
                "Get All Shifts",
                []
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching shifts from API");
            return new ApiResponseDto<List<Shift>>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Data = [],
                RequestFailed = true
            };
        }
    }

    public async Task<ApiResponseDto<Shift?>> GetShiftByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/shifts/{id}").ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<Shift?>(
                response,
                _logger,
                $"Get Shift {id}",
                null
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching shift {ShiftId}", id);
            return new ApiResponseDto<Shift?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Shift>> CreateShiftAsync(Shift shift)
    {
        // No DTO conversion needed - send the model directly!
        // Backend validation handles all checks now
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/shifts", shift).ConfigureAwait(false);
            var handled = await HttpResponseHelper.HandleHttpResponseAsync<Shift>(
                response,
                _logger,
                "Create Shift",
                shift
            ).ConfigureAwait(false);

            // If server returned the created Shift but omitted navigation properties, fetch it by ID
            if (!handled.RequestFailed && handled.Data != null && (handled.Data.Worker == null || handled.Data.Location == null))
            {
                var refreshed = await GetShiftByIdAsync(handled.Data.ShiftId).ConfigureAwait(false);
                if (!refreshed.RequestFailed && refreshed.Data != null)
                {
                    return new ApiResponseDto<Shift>(refreshed.Message ?? "Shift retrieved")
                    {
                        Data = refreshed.Data,
                        RequestFailed = false,
                        ResponseCode = refreshed.ResponseCode,
                        TotalCount = refreshed.TotalCount
                    };
                }
            }

            return handled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating shift");
            return new ApiResponseDto<Shift>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Shift?>> UpdateShiftAsync(int id, Shift updatedShift)
    {
        var dto = new ConsoleFrontEnd.Models.Dtos.ShiftApiRequestDto
        {
            WorkerId = updatedShift.WorkerId,
            StartTime = updatedShift.StartTime,
            EndTime = updatedShift.EndTime,
            LocationId = updatedShift.LocationId
        };
        var errors = Services.Validation.ShiftValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Shift?>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/shifts/{id}", dto).ConfigureAwait(false);
            var handled = await HttpResponseHelper.HandleHttpResponseAsync<Shift?>(
                response,
                _logger,
                $"Update Shift {id}",
                updatedShift
            ).ConfigureAwait(false);

            // If server returned only IDs (no navigation properties), fetch the fully populated shift
            if (!handled.RequestFailed && handled.Data != null && (handled.Data.Worker == null || handled.Data.Location == null))
            {
                var refreshed = await GetShiftByIdAsync(id);
                if (!refreshed.RequestFailed && refreshed.Data != null)
                    return refreshed;
            }

            return handled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating shift {ShiftId}", id);
            return new ApiResponseDto<Shift?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteShiftAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/shifts/{id}").ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<bool>(
                response,
                _logger,
                $"Delete Shift {id}",
                false
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting shift {ShiftId}", id);
            return new ApiResponseDto<bool>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = false
            };
        }
    }
}
