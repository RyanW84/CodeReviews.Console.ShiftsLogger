using System.Net;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Services;

public class LocationService(ILocationRepository locationRepository) : ILocationService
{
    private readonly ILocationRepository _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

    public async Task<ApiResponseDto<List<Location>>> GetAllLocations(
        LocationFilterOptions locationOptions
    )
    {
        var result = await _locationRepository.GetAllAsync(locationOptions).ConfigureAwait(false);
        return new ApiResponseDto<List<Location>>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.IsFailure ? result.StatusCode : System.Net.HttpStatusCode.OK,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<Location>> GetLocationById(int id)
    {
        var result = await _locationRepository.GetByIdAsync(id).ConfigureAwait(false);
    if (result.IsFailure || result.Data is null)
            return new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = result.StatusCode,
                Message = result.Message,
                Data = null
            };

        return new ApiResponseDto<Location>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<Location>> CreateLocation(LocationApiRequestDto location)
    {
        try
        {
            var result = await _locationRepository.CreateAsync(location).ConfigureAwait(false);
            if (result.IsFailure)
                return new ApiResponseDto<Location>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                };

            return new ApiResponseDto<Location>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.Created,
                Message = result.Message,
                Data = result.Data
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Back end location service - {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Location>> UpdateLocation(
        int id,
        LocationApiRequestDto updatedLocation
    )
    {
        var result = await _locationRepository.UpdateAsync(id, updatedLocation).ConfigureAwait(false);
        if (result.IsFailure)
            return new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = result.StatusCode,
                Message = result.Message,
                Data = null
            };

        return new ApiResponseDto<Location>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<string?>> DeleteLocation(int id)
    {
        var result = await _locationRepository.DeleteAsync(id).ConfigureAwait(false);
        return new ApiResponseDto<string?>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.StatusCode,
            Message = result.Message,
            Data = null
        };
    }
}