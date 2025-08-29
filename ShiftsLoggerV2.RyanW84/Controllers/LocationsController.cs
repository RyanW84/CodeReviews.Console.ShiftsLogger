using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;
using ShiftsLoggerV2.RyanW84.Common;

namespace ShiftsLoggerV2.RyanW84.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : BaseController
{
    private readonly ILocationBusinessService _locationBusinessService;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(ILocationBusinessService locationBusinessService, ILogger<LocationsController> logger)
    {
        _locationBusinessService = locationBusinessService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedApiResponseDto<List<Location>>>> GetAllLocations([FromQuery] LocationFilterOptions locationOptions)
    {
        try
        {
            var result = await _locationBusinessService.GetAllAsync(locationOptions);
            return HandlePaginatedResult(result, locationOptions, "Locations retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all locations");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new PaginatedApiResponseDto<List<Location>>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null,
                TotalCount = 0,
                PageNumber = locationOptions.PageNumber,
                PageSize = locationOptions.PageSize
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<Location>>> GetLocationById(int id)
    {
        try
        {
            var result = await _locationBusinessService.GetByIdAsync(id);
            return HandleResult<Location>(result, "Location retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve location by ID {LocationId}", id);
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Location>>> CreateLocation([FromBody] LocationApiRequestDto location)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            var result = await _locationBusinessService.CreateAsync(location);
            return HandleResult<Location>(result, "Location created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateLocation failed with exception");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Location>>> UpdateLocation([FromRoute] int id, [FromBody] LocationApiRequestDto updatedLocation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            var result = await _locationBusinessService.UpdateAsync(id, updatedLocation);
            return HandleResult<Location>(result, "Location updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateLocation failed for ID {LocationId}", id);
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteLocation(int id)
    {
        try
        {
            var result = await _locationBusinessService.DeleteAsync(id);
            return HandleNonGenericResult(result, "Location deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteLocation failed for ID {LocationId}", id);
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<string>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = string.Empty
            });
        }
    }
}
