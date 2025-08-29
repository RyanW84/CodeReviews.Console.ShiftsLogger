using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

/// <summary>
/// Location Service for API calls - merged and cleaned with comprehensive error handling
/// </summary>
public class LocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocationService> _logger;

    public LocationService(HttpClient httpClient, ILogger<LocationService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponseDto<List<Location>>> GetLocationsByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.LocationFilterOptions filter, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var queryString = $"api/locations?" + BuildLocationFilterQuery(filter, pageNumber, pageSize);
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Location>>(
                response,
                _logger,
                "Get Locations By Filter",
                []
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering locations via API");
            return new ApiResponseDto<List<Location>>($"Filter Error: {ex.Message}")
            {
                Data = [],
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    private string BuildLocationFilterQuery(ConsoleFrontEnd.Models.FilterOptions.LocationFilterOptions filter, int pageNumber = 1, int pageSize = 10)
    {
        List<string> query = [];
        if (filter.LocationId.HasValue) query.Add($"LocationId={filter.LocationId.Value}");
        if (!string.IsNullOrWhiteSpace(filter.Name)) query.Add($"Name={Uri.EscapeDataString(filter.Name)}");
        if (!string.IsNullOrWhiteSpace(filter.Address)) query.Add($"Address={Uri.EscapeDataString(filter.Address)}");
        if (!string.IsNullOrWhiteSpace(filter.Town)) query.Add($"Town={Uri.EscapeDataString(filter.Town)}");
        if (!string.IsNullOrWhiteSpace(filter.County)) query.Add($"County={Uri.EscapeDataString(filter.County)}");
        if (!string.IsNullOrWhiteSpace(filter.PostCode)) query.Add($"PostCode={Uri.EscapeDataString(filter.PostCode)}");
        if (!string.IsNullOrWhiteSpace(filter.Country)) query.Add($"Country={Uri.EscapeDataString(filter.Country)}");
        if (!string.IsNullOrWhiteSpace(filter.Search)) query.Add($"Search={Uri.EscapeDataString(filter.Search)}");

        // Add pagination parameters
        query.Add($"pageNumber={pageNumber}");
        query.Add($"pageSize={pageSize}");

        return string.Join("&", query);
    }

    public async Task<ApiResponseDto<List<Location>>> GetAllLocationsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var queryString = $"api/locations?pageNumber={pageNumber}&pageSize={pageSize}";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Location>>(
                response,
                _logger,
                "Get All Locations",
                []
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching locations from API");
            return new ApiResponseDto<List<Location>>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Data = [],
                RequestFailed = true
            };
        }
    }

    public async Task<ApiResponseDto<Location?>> GetLocationByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/locations/{id}").ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<Location?>(
                response,
                _logger,
                $"Get Location {id}",
                null
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching location {LocationId}", id);
            return new ApiResponseDto<Location?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Location?>> GetLocationByNameAsync(string name)
    {
        try
        {
            // Use filter to find location by name efficiently
            var filter = new ConsoleFrontEnd.Models.FilterOptions.LocationFilterOptions { Name = name };
            var response = await GetLocationsByFilterAsync(filter);
            
            if (response.RequestFailed || response.Data == null)
                return new ApiResponseDto<Location?>(response.Message ?? "Location not found") 
                { 
                    RequestFailed = true, 
                    ResponseCode = response.ResponseCode,
                    Data = null 
                };

            var location = response.Data.FirstOrDefault();
            return new ApiResponseDto<Location?>(location != null ? "Location found" : "Location not found")
            {
                Data = location,
                RequestFailed = location == null,
                ResponseCode = location != null ? HttpStatusCode.OK : HttpStatusCode.NotFound
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching location by name {LocationName}", name);
            return new ApiResponseDto<Location?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Location>> CreateLocationAsync(Location location)
    {
        var dto = new ConsoleFrontEnd.Models.Dtos.LocationApiRequestDto
        {
            Name = location.Name,
            Address = location.Address,
            Town = location.Town,
            County = location.County,
            PostCode = location.PostCode,
            Country = location.Country
        };
        var errors = Services.Validation.LocationValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Location>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/locations", dto).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<Location>(
                response,
                _logger,
                "Create Location",
                location
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating location");
            return new ApiResponseDto<Location>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Location?>> UpdateLocationAsync(int id, Location updatedLocation)
    {
        var dto = new ConsoleFrontEnd.Models.Dtos.LocationApiRequestDto
        {
            Name = updatedLocation.Name,
            Address = updatedLocation.Address,
            Town = updatedLocation.Town,
            County = updatedLocation.County,
            PostCode = updatedLocation.PostCode,
            Country = updatedLocation.Country
        };
        var errors = Services.Validation.LocationValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Location?>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/locations/{id}", dto).ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<Location?>(
                response,
                _logger,
                $"Update Location {id}",
                updatedLocation
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating location {LocationId}", id);
            return new ApiResponseDto<Location?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteLocationAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/locations/{id}").ConfigureAwait(false);
            return await HttpResponseHelper.HandleHttpResponseAsync<bool>(
                response,
                _logger,
                $"Delete Location {id}",
                false
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting location {LocationId}", id);
            return new ApiResponseDto<bool>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = false
            };
        }
    }
}
