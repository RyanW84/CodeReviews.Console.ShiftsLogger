using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Business;
using ConsoleFrontEnd.Services.Common;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services.Controllers;

/// <summary>
/// Console controller service that provides controller-like functionality
/// without HTTP dependencies. Acts as a bridge between console UI and business services.
/// </summary>
public class ShiftControllerService : IShiftControllerService
{
    private readonly IShiftService _shiftService;
    private readonly IWorkerService _workerService;
    private readonly ILocationService _locationService;
    private readonly IErrorHandlingService _errorHandlingService;
    private readonly ILogger<ShiftControllerService> _logger;

    public ShiftControllerService(
        IShiftService shiftService,
        IWorkerService workerService,
        ILocationService locationService,
        IErrorHandlingService errorHandlingService,
        ILogger<ShiftControllerService> logger)
    {
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<PaginatedApiResponseDto<List<Shift>>>> GetAllShiftsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting all shifts - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            var result = await _shiftService.GetAllShiftsAsync(pageNumber, pageSize);

            if (result.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    result.ResponseCode, "retrieve shifts", result.Message);
                return OperationResult<PaginatedApiResponseDto<List<Shift>>>.Failure(errorMessage);
            }

            var response = new PaginatedApiResponseDto<List<Shift>>
            {
                RequestFailed = false,
                Message = "Shifts retrieved successfully",
                Data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            return OperationResult<PaginatedApiResponseDto<List<Shift>>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shifts");
            return OperationResult<PaginatedApiResponseDto<List<Shift>>>.Failure("An unexpected error occurred while retrieving shifts.");
        }
    }

    public async Task<OperationResult<Shift>> GetShiftByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting shift by ID: {ShiftId}", id);

            var result = await _shiftService.GetShiftByIdAsync(id);

            if (result.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    result.ResponseCode, "retrieve shift", result.Message);
                return OperationResult<Shift>.Failure(errorMessage);
            }

            if (result.Data == null)
            {
                return OperationResult<Shift>.Failure($"Shift with ID {id} not found");
            }

            return OperationResult<Shift>.Success(result.Data, "Shift retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift by ID {ShiftId}", id);
            return OperationResult<Shift>.Failure("An unexpected error occurred while retrieving the shift");
        }
    }

    public async Task<OperationResult<Shift>> CreateShiftAsync(ShiftApiRequestDto shiftDto)
    {
        try
        {
            _logger.LogInformation("Creating new shift: {@ShiftDto}", shiftDto);

            // Validate input
            if (shiftDto == null)
            {
                return OperationResult<Shift>.Failure("Shift data is required");
            }

            // Validate worker exists
            var workerResult = await _workerService.GetWorkerByIdAsync(shiftDto.WorkerId);
            if (workerResult.RequestFailed || workerResult.Data == null)
            {
                return OperationResult<Shift>.Failure("Invalid worker ID");
            }

            // Validate location exists
            var locationResult = await _locationService.GetLocationByIdAsync(shiftDto.LocationId);
            if (locationResult.RequestFailed || locationResult.Data == null)
            {
                return OperationResult<Shift>.Failure("Invalid location ID");
            }

            // Create the shift object
            var shift = new Shift
            {
                WorkerId = shiftDto.WorkerId,
                StartTime = shiftDto.StartTime,
                EndTime = shiftDto.EndTime,
                LocationId = shiftDto.LocationId,
                Worker = workerResult.Data,
                Location = locationResult.Data
            };

            // Create the shift
            var createResult = await _shiftService.CreateShiftAsync(shift);

            if (createResult.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    createResult.ResponseCode, "create shift", createResult.Message);
                return OperationResult<Shift>.Failure(errorMessage);
            }

            return OperationResult<Shift>.Success(createResult.Data!, "Shift created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift: {@ShiftDto}", shiftDto);
            return OperationResult<Shift>.Failure("An unexpected error occurred while creating the shift");
        }
    }

    public async Task<OperationResult<Shift>> UpdateShiftAsync(int id, ShiftApiRequestDto shiftDto)
    {
        try
        {
            _logger.LogInformation("Updating shift {ShiftId}: {@ShiftDto}", id, shiftDto);

            if (shiftDto == null)
            {
                return OperationResult<Shift>.Failure("Shift data is required");
            }

            // Check if shift exists
            var existingResult = await _shiftService.GetShiftByIdAsync(id);
            if (existingResult.RequestFailed || existingResult.Data == null)
            {
                return OperationResult<Shift>.Failure($"Shift with ID {id} not found");
            }

            // Validate worker exists
            var workerResult = await _workerService.GetWorkerByIdAsync(shiftDto.WorkerId);
            if (workerResult.RequestFailed || workerResult.Data == null)
            {
                return OperationResult<Shift>.Failure("Invalid worker ID");
            }

            // Validate location exists
            var locationResult = await _locationService.GetLocationByIdAsync(shiftDto.LocationId);
            if (locationResult.RequestFailed || locationResult.Data == null)
            {
                return OperationResult<Shift>.Failure("Invalid location ID");
            }

            // Create the updated shift object
            var updatedShift = new Shift
            {
                ShiftId = id,
                WorkerId = shiftDto.WorkerId,
                StartTime = shiftDto.StartTime,
                EndTime = shiftDto.EndTime,
                LocationId = shiftDto.LocationId,
                Worker = workerResult.Data,
                Location = locationResult.Data
            };

            // Update the shift
            var updateResult = await _shiftService.UpdateShiftAsync(id, updatedShift);

            if (updateResult.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    updateResult.ResponseCode, "update shift", updateResult.Message);
                return OperationResult<Shift>.Failure(errorMessage);
            }

            return OperationResult<Shift>.Success(updateResult.Data!, "Shift updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift {ShiftId}: {@ShiftDto}", id, shiftDto);
            return OperationResult<Shift>.Failure("An unexpected error occurred while updating the shift");
        }
    }

    public async Task<OperationResult> DeleteShiftAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting shift {ShiftId}", id);

            // Check if shift exists
            var existingResult = await _shiftService.GetShiftByIdAsync(id);
            if (existingResult.RequestFailed || existingResult.Data == null)
            {
                return OperationResult.Failure($"Shift with ID {id} not found");
            }

            // Delete the shift
            var deleteResult = await _shiftService.DeleteShiftAsync(id);

            if (deleteResult.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    deleteResult.ResponseCode, "delete shift", deleteResult.Message);
                return OperationResult.Failure(errorMessage);
            }

            return OperationResult.Success("Shift deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift {ShiftId}", id);
            return OperationResult.Failure("An unexpected error occurred while deleting the shift");
        }
    }

    public async Task<OperationResult<List<Shift>>> FilterShiftsAsync(ShiftFilterOptions options)
    {
        try
        {
            _logger.LogInformation("Filtering shifts with options: {@Options}", options);

            var result = await _shiftService.GetShiftsByFilterAsync(options);

            if (result.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    result.ResponseCode, "filter shifts", result.Message);
                return OperationResult<List<Shift>>.Failure(errorMessage);
            }

            return OperationResult<List<Shift>>.Success(result.Data ?? new List<Shift>(), "Shifts filtered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering shifts with options: {@Options}", options);
            return OperationResult<List<Shift>>.Failure("An unexpected error occurred while filtering shifts");
        }
    }
}
