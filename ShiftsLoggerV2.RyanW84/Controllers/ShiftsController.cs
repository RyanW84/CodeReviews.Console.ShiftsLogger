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
public class ShiftsController : BaseController
{
    private readonly IShiftBusinessService _shiftBusinessService;
    private readonly ILogger<ShiftsController> _logger;

    public ShiftsController(IShiftBusinessService shiftBusinessService, ILogger<ShiftsController> logger)
    {
        _shiftBusinessService = shiftBusinessService;
        _logger = logger;
    }



    // This is the route for getting all shifts
    [HttpGet(Name = "Get All Shifts")]
    public async Task<ActionResult<PaginatedApiResponseDto<List<Shift>>>> GetAllShifts([FromQuery] ShiftFilterOptions shiftOptions)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _shiftBusinessService.GetAllAsync(shiftOptions);
            return HandlePaginatedResult(result, shiftOptions, "Shifts retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all shifts");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new PaginatedApiResponseDto<List<Shift>>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null,
                TotalCount = 0,
                PageNumber = shiftOptions.PageNumber,
                PageSize = shiftOptions.PageSize
            });
        }
    }

    // This is the route for getting a createdShift by ID
    [HttpGet("{id}")] // This will be added to the API URI (send some data during the request
    public async Task<ActionResult<ApiResponseDto<Shift>>> GetShiftById(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _shiftBusinessService.GetByIdAsync(id);
            return HandleResult<Shift>(result, "Shift retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve shift by ID {ShiftId}", id);
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Shift>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }
    // This is the route for creating a createdShift
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Shift>>> CreateShift([FromBody] ShiftApiRequestDto shift)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateShift failed due to invalid model state: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequestModelState();
            }

            // Ensure end is after start (JSON converter handles parsing automatically)
            if (shift.EndTime <= shift.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
                return BadRequestModelState();
            }

            var result = await _shiftBusinessService.CreateAsync(shift);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("CreateShift validation failed: {Message}", result.Message);
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            _logger.LogInformation("Shift created successfully for WorkerId={WorkerId}, LocationId={LocationId}, Start={StartTime}, End={EndTime}",
                shift.WorkerId, shift.LocationId, shift.StartTime, shift.EndTime);
            return StatusCode(201, new ApiResponseDto<Shift>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.Created,
                Message = "Shift created successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateShift failed with exception");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Shift>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    // This is the route for updating a createdShift
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Shift>>> UpdateShift([FromRoute] int id, [FromBody] ShiftApiRequestDto shift)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequestModelState();

            // Ensure end is after start (JSON converter handles parsing automatically)
            if (shift.EndTime <= shift.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
                return BadRequestModelState();
            }

            var result = await _shiftBusinessService.UpdateAsync(id, shift);

            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<Shift>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shift updated successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update shift failed, see exception {ex}");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Shift>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteShift([FromRoute] int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _shiftBusinessService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = false
                });
            }

            return Ok(new ApiResponseDto<bool>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shift deleted successfully",
                Data = true,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete shift failed, see exception {ex}");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = false
            });
        }
    }

    // Additional V5 endpoints - Enhanced functionality from SOLID implementation

    [HttpGet("by-date-range")]
    public async Task<ActionResult<ApiResponseDto<List<Shift>>>> GetShiftsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var filterOptions = new ShiftFilterOptions
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _shiftBusinessService.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Shift>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Shift>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shifts retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get shifts by date range failed: {ex}");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<List<Shift>>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }

    [HttpGet("worker/{workerId}")]
    public async Task<ActionResult<ApiResponseDto<List<Shift>>>> GetShiftsByWorker(int workerId)
    {
        try
        {
            var filterOptions = new ShiftFilterOptions { WorkerId = workerId };

            var result = await _shiftBusinessService.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Shift>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Shift>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shifts retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get shifts by worker failed: {ex}");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<List<Shift>>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }
}