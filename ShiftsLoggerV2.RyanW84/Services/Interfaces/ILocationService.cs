using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;

namespace ShiftsLoggerV2.RyanW84.Services;

public interface ILocationService
{
    public Task<ApiResponseDto<List<Location>>> GetAllLocations(
        LocationFilterOptions locationOptions
    );

    public Task<ApiResponseDto<Location>> GetLocationById(int id);
    public Task<ApiResponseDto<Location>> CreateLocation(LocationApiRequestDto locationDto);

    public Task<ApiResponseDto<Location>> UpdateLocation(
        int id,
        LocationApiRequestDto updatedLocation
    );

    public Task<ApiResponseDto<string?>> DeleteLocation(int id);
}