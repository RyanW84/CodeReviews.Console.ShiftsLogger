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
public class WorkersController : BaseController
{
    private readonly IWorkerBusinessService _workerBusinessService;
    private readonly ILogger<WorkersController> _logger;

    public WorkersController(IWorkerBusinessService workerBusinessService, ILogger<WorkersController> logger)
    {
        _workerBusinessService = workerBusinessService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedApiResponseDto<List<Worker>>>> GetAllWorkers([FromQuery] WorkerFilterOptions workerOptions)
    {
        try
        {
            var result = await _workerBusinessService.GetAllAsync(workerOptions);
            return HandlePaginatedResult(result, workerOptions, "Workers retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all workers");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new PaginatedApiResponseDto<List<Worker>>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null,
                TotalCount = 0,
                PageNumber = workerOptions.PageNumber,
                PageSize = workerOptions.PageSize
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<Worker>>> GetWorkerById(int id)
    {
        try
        {
            var result = await _workerBusinessService.GetByIdAsync(id);
            return HandleResult<Worker>(result, "Worker retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve worker by ID {WorkerId}", id);
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Worker>>> CreateWorker([FromBody] WorkerApiRequestDto worker)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            var result = await _workerBusinessService.CreateAsync(worker);
            return HandleResult<Worker>(result, "Worker created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorker failed with exception");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Worker>>> UpdateWorker([FromRoute] int id, [FromBody] WorkerApiRequestDto updatedWorker)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            var result = await _workerBusinessService.UpdateAsync(id, updatedWorker);
            return HandleResult<Worker>(result, "Worker updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateWorker failed for ID {WorkerId}", id);
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteWorker(int id)
    {
        try
        {
            var result = await _workerBusinessService.DeleteAsync(id);
            return HandleNonGenericResult(result, "Worker deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteWorker failed for ID {WorkerId}", id);
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
