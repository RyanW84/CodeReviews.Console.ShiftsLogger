using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Business;
using ConsoleFrontEnd.Services.Common;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services.Infrastructure;

/// <summary>
/// Orchestrates shift business operations
/// Follows Single Responsibility Principle by handling business workflows
/// Implements Dependency Inversion Principle through interfaces
/// </summary>
public class ShiftOrchestrationService : IShiftOrchestrationService
{
    private readonly IShiftService _shiftService;
    private readonly IWorkerService _workerService;
    private readonly ILocationService _locationService;
    private readonly IErrorHandlingService _errorHandlingService;
    private readonly ShiftInputHelper _shiftInputHelper;
    private readonly ILogger<ShiftOrchestrationService> _logger;

    public ShiftOrchestrationService(
        IShiftService shiftService,
        IWorkerService workerService,
        ILocationService locationService,
        IErrorHandlingService errorHandlingService,
        ShiftInputHelper shiftInputHelper,
        ILogger<ShiftOrchestrationService> logger)
    {
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
        _shiftInputHelper = shiftInputHelper ?? throw new ArgumentNullException(nameof(shiftInputHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<Shift>> CreateShiftWorkflowAsync()
    {
        try
        {
            // Step 1: Ensure prerequisites exist
            var prerequisitesResult = await ValidatePrerequisitesAsync();
            if (!prerequisitesResult.IsSuccess)
                return OperationResult<Shift>.Failure(prerequisitesResult.Message, prerequisitesResult.Errors.ToArray());

            // Step 2: Collect user input
            var inputResult = await CollectShiftInputAsync();
            if (!inputResult.IsSuccess)
                return OperationResult<Shift>.Failure(inputResult.Message, inputResult.Errors.ToArray());

            // Step 3: Validate business rules
            var validationResult = ValidateShiftBusinessRules(inputResult.Data!);
            if (!validationResult.IsSuccess)
                return OperationResult<Shift>.Failure(validationResult.Message, validationResult.Errors.ToArray());

            // Step 4: Create the shift
            var createResponse = await _shiftService.CreateShiftAsync(inputResult.Data!);
            if (createResponse.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    createResponse.ResponseCode, "create shift", createResponse.Message);
                return OperationResult<Shift>.Failure(errorMessage);
            }

            return OperationResult<Shift>.Success(createResponse.Data!, "Shift created successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in shift creation workflow");
            return OperationResult<Shift>.Failure("An unexpected error occurred during shift creation.");
        }
    }

    public async Task<OperationResult<Shift>> UpdateShiftWorkflowAsync(int shiftId)
    {
        try
        {
            // Step 1: Get existing shift
            var existingResponse = await _shiftService.GetShiftByIdAsync(shiftId);
            if (existingResponse.RequestFailed || existingResponse.Data == null)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    existingResponse.ResponseCode, "retrieve shift", existingResponse.Message);
                return OperationResult<Shift>.Failure(errorMessage);
            }

            // Step 2: Collect updated input
            var inputResult = await CollectShiftInputAsync(existingResponse.Data);
            if (!inputResult.IsSuccess)
                return OperationResult<Shift>.Failure(inputResult.Message, inputResult.Errors.ToArray());

            // Step 3: Validate business rules
            var validationResult = ValidateShiftBusinessRules(inputResult.Data!);
            if (!validationResult.IsSuccess)
                return OperationResult<Shift>.Failure(validationResult.Message, validationResult.Errors.ToArray());

            // Step 4: Update the shift
            var updateResponse = await _shiftService.UpdateShiftAsync(shiftId, inputResult.Data!);
            if (updateResponse.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    updateResponse.ResponseCode, "update shift", updateResponse.Message);
                return OperationResult<Shift>.Failure(errorMessage);
            }

            return OperationResult<Shift>.Success(updateResponse.Data!, "Shift updated successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in shift update workflow");
            return OperationResult<Shift>.Failure("An unexpected error occurred during shift update.");
        }
    }

    public async Task<OperationResult> DeleteShiftWorkflowAsync(int shiftId)
    {
        try
        {
            // Step 1: Verify shift exists
            var existingResponse = await _shiftService.GetShiftByIdAsync(shiftId);
            if (existingResponse.RequestFailed || existingResponse.Data == null)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    existingResponse.ResponseCode, "retrieve shift", existingResponse.Message);
                return OperationResult.Failure(errorMessage);
            }

            // Step 2: Delete the shift
            var deleteResponse = await _shiftService.DeleteShiftAsync(shiftId);
            if (deleteResponse.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    deleteResponse.ResponseCode, "delete shift", deleteResponse.Message);
                return OperationResult.Failure(errorMessage);
            }

            return OperationResult.Success("Shift deleted successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in shift deletion workflow");
            return OperationResult.Failure("An unexpected error occurred during shift deletion.");
        }
    }

    public async Task<OperationResult<List<Shift>>> FilterShiftsWorkflowAsync()
    {
        try
        {
            // Step 1: Collect filter criteria
            var filterResult = await CollectFilterCriteriaAsync();
            if (!filterResult.IsSuccess)
                return OperationResult<List<Shift>>.Failure(filterResult.Message, filterResult.Errors.ToArray());

            // Step 2: Apply filters
            var response = await _shiftService.GetShiftsByFilterAsync(filterResult.Data!);
            if (response.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    response.ResponseCode, "filter shifts", response.Message);
                return OperationResult<List<Shift>>.Failure(errorMessage);
            }

            if (response.Data == null || !response.Data.Any())
            {
                return OperationResult<List<Shift>>.Failure("No shifts found matching the specified criteria.");
            }

            return OperationResult<List<Shift>>.Success(response.Data.ToList(),
                $"Found {response.TotalCount} shifts matching your criteria.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in shift filtering workflow");
            return OperationResult<List<Shift>>.Failure("An unexpected error occurred during shift filtering.");
        }
    }

    public async Task<OperationResult<List<Shift>>> GetShiftsByWorkerWorkflowAsync(int workerId)
    {
        try
        {
            var filter = new ShiftFilterOptions { WorkerId = workerId };
            var response = await _shiftService.GetShiftsByFilterAsync(filter);
            if (response.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    response.ResponseCode, "get shifts by worker", response.Message);
                return OperationResult<List<Shift>>.Failure(errorMessage);
            }

            if (response.Data == null || !response.Data.Any())
            {
                return OperationResult<List<Shift>>.Failure("No shifts found for the specified worker.");
            }

            return OperationResult<List<Shift>>.Success(response.Data.ToList(),
                $"Found {response.TotalCount} shifts for the worker.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shifts by worker");
            return OperationResult<List<Shift>>.Failure("An unexpected error occurred while retrieving shifts.");
        }
    }

    public async Task<OperationResult<List<Shift>>> GetShiftsByDateRangeWorkflowAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            // Validate date range
            if (endDate <= startDate)
            {
                return OperationResult<List<Shift>>.Failure("End date must be after start date.");
            }

            var filter = new ShiftFilterOptions
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var response = await _shiftService.GetShiftsByFilterAsync(filter);
            if (response.RequestFailed)
            {
                var errorMessage = _errorHandlingService.GetUserFriendlyErrorMessage(
                    response.ResponseCode, "get shifts by date range", response.Message);
                return OperationResult<List<Shift>>.Failure(errorMessage);
            }

            if (response.Data == null || !response.Data.Any())
            {
                return OperationResult<List<Shift>>.Failure("No shifts found in the specified date range.");
            }

            return OperationResult<List<Shift>>.Success(response.Data.ToList(),
                $"Found {response.TotalCount} shifts in the date range.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shifts by date range");
            return OperationResult<List<Shift>>.Failure("An unexpected error occurred while retrieving shifts.");
        }
    }

    #region Private Helper Methods

    private async Task<OperationResult> ValidatePrerequisitesAsync()
    {
        var errors = new List<string>();

        // Check workers exist
        var workersResponse = await _workerService.GetAllWorkersAsync();
        if (workersResponse.RequestFailed || workersResponse.Data == null || !workersResponse.Data.Any())
        {
            errors.Add("No workers found. Please create workers first.");
        }

        // Check locations exist
        var locationsResponse = await _locationService.GetAllLocationsAsync();
        if (locationsResponse.RequestFailed || locationsResponse.Data == null || !locationsResponse.Data.Any())
        {
            errors.Add("No locations found. Please create locations first.");
        }

        return errors.Any()
            ? OperationResult.Failure("Prerequisites not met", errors.ToArray())
            : OperationResult.Success();
    }

    private async Task<OperationResult<Shift>> CollectShiftInputAsync(Shift? existingShift = null)
    {
        try
        {
            var workerId = await _shiftInputHelper.SelectWorkerAsync(existingShift?.WorkerId, existingShift != null);
            if (workerId <= 0)
                return OperationResult<Shift>.Failure("No worker selected.");

            var locationId = await _shiftInputHelper.SelectLocationAsync(existingShift?.LocationId, existingShift != null);
            if (locationId <= 0)
                return OperationResult<Shift>.Failure("No location selected.");

            var startTime = await _shiftInputHelper.GetDateTimeInputAsync("Start Time", existingShift?.StartTime, existingShift != null);
            var endTime = await _shiftInputHelper.GetDateTimeInputAsync("End Time", existingShift?.EndTime, existingShift != null);

            var shift = new Shift
            {
                WorkerId = workerId,
                LocationId = locationId,
                StartTime = startTime,
                EndTime = endTime
            };

            return OperationResult<Shift>.Success(shift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting shift input");
            return OperationResult<Shift>.Failure("Error collecting input from user.");
        }
    }

    private static OperationResult ValidateShiftBusinessRules(Shift shift)
    {
        var errors = new List<string>();

        if (shift.EndTime <= shift.StartTime)
            errors.Add("End time must be after start time.");

        if (shift.WorkerId <= 0)
            errors.Add("Valid worker must be selected.");

        if (shift.LocationId <= 0)
            errors.Add("Valid location must be selected.");

        return errors.Any()
            ? OperationResult.Failure("Validation failed", errors.ToArray())
            : OperationResult.Success();
    }

    private async Task<OperationResult<ShiftFilterOptions>> CollectFilterCriteriaAsync()
    {
        try
        {
            var filter = new ShiftFilterOptions();

            // Worker filter
            var filterByWorker = await _shiftInputHelper.GetConfirmationAsync("Filter by worker?");
            if (filterByWorker)
            {
                filter.WorkerId = await _shiftInputHelper.SelectWorkerAsync(null, false);
                if (filter.WorkerId <= 0)
                    return OperationResult<ShiftFilterOptions>.Failure("No worker selected.");
            }

            // Location filter
            var filterByLocation = await _shiftInputHelper.GetConfirmationAsync("Filter by location?");
            if (filterByLocation)
            {
                filter.LocationId = await _shiftInputHelper.SelectLocationAsync(null, false);
                if (filter.LocationId <= 0)
                    return OperationResult<ShiftFilterOptions>.Failure("No location selected.");
            }

            // Date range filter
            var filterByDate = await _shiftInputHelper.GetConfirmationAsync("Filter by date range?");
            if (filterByDate)
            {
                filter.StartDate = (await _shiftInputHelper.GetDateTimeInputAsync("Start Date")).DateTime;
                filter.EndDate = (await _shiftInputHelper.GetDateTimeInputAsync("End Date")).DateTime;

                if (filter.EndDate <= filter.StartDate)
                    return OperationResult<ShiftFilterOptions>.Failure("End date must be after start date.");
            }

            return OperationResult<ShiftFilterOptions>.Success(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting filter criteria");
            return OperationResult<ShiftFilterOptions>.Failure("Error collecting filter criteria from user.");
        }
    }

    #endregion
}
