using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Services.Business;
using ConsoleFrontEnd.Services.Common;
using ConsoleFrontEnd.Services.Controllers;
using ConsoleFrontEnd.Services.Display;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
///     Shift menu implementation following Single Responsibility Principle
///     Handles shift-specific operations
/// </summary>
public class ShiftMenu : BaseMenu
{
    // Configuration constants
    private const int DefaultPageSize = 50;

    // Use inherited DisplayService, InputService, NavigationService and Logger from BaseMenu
    // Private fields for services specific to this menu (use underscore names because methods reference them)
    private readonly IShiftUi _shiftUi;
    private readonly IWorkerUi _workerUi;
    private readonly IShiftControllerService _controllerService;
    private readonly IShiftDisplayService _shiftDisplayService;
    private readonly Dictionary<string, Func<Task<bool>>> MenuActions;

    public ShiftMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<ShiftMenu> logger,
        IShiftUi shiftUi,
        IWorkerUi workerUi,
        IShiftControllerService controllerService,
        IShiftDisplayService shiftDisplayService
    )
        : base(displayService, inputService, navigationService, logger)
    {
        // base constructor sets DisplayService, InputService, NavigationService and Logger
        _shiftUi = shiftUi ?? throw new ArgumentNullException(nameof(shiftUi));
        _workerUi = workerUi ?? throw new ArgumentNullException(nameof(workerUi));
        _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        _shiftDisplayService = shiftDisplayService ?? throw new ArgumentNullException(nameof(shiftDisplayService));

        MenuActions = new Dictionary<string, Func<Task<bool>>>
        {
            ["View All Shifts"] = async () => { await ViewAllShiftsAsync(); return false; },
            ["View Shift by ID"] = async () => { await ViewShiftByIdAsync(); return false; },
            ["Create New Shift"] = async () => { await CreateShiftAsync(); return false; },
            ["Update Shift"] = async () => { await UpdateShiftAsync(); return false; },
            ["Delete Shift"] = async () => { await DeleteShiftAsync(); return false; },
            ["Filter Shifts"] = async () => { await FilterShiftsAsync(); return false; },
            ["Back to Main Menu"] = () => Task.FromResult(true)
        };
    }

    public override string Title => "Shift Management";
    public override string Context => "Shift Management";

    protected override async Task ShowMenuAsync()
    {
        var shouldExit = false;

        while (!shouldExit)
        {
            var choice = await InputService.GetMenuChoiceAsync(
                "Select a shift operation:",
                "View All Shifts",
                "View Shift by ID",
                "Create New Shift",
                "Update Shift",
                "Delete Shift",
                "Filter Shifts",
                "Back to Main Menu"
            );

            shouldExit = await HandleShiftChoice(choice);
        }
    }

    private async Task<bool> HandleShiftChoice(string choice)
    {
        Logger.LogDebug("Shift menu choice selected: {Choice}", choice);

        if (await HandleCommonActions(choice))
            return true;

        if (MenuActions.TryGetValue(choice, out var action))
        {
            return await action();
        }
        else
        {
            DisplayService.DisplayError("Invalid choice");
            await InputService.WaitForKeyPressAsync();
            return false;
        }
    }

    private async Task ViewAllShiftsAsync()
    {
        try
        {
            await _shiftUi.DisplayShiftsWithPaginationAsync(1, DefaultPageSize);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing all shifts");
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
        }
    }

    private async Task ViewShiftByIdAsync()
    {
        DisplayService.DisplayHeader("View Shift by ID", "blue");

        var shiftId = await _shiftUi.GetShiftByIdUi();

        if (shiftId <= 0)
        {
            DisplayService.DisplayInfo("Operation cancelled.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        await HandleOperationAsync(
            "View Shift by ID",
            () => _controllerService.GetShiftByIdAsync(shiftId),
            shift => _shiftDisplayService.DisplayShiftDetailsAsync(shift).Wait(),
            "blue"
        );
    }

    private async Task CreateShiftAsync()
    {
        DisplayService.DisplayHeader("Create New Shift", "green");

        try
        {
            // Get worker selection
            var workerId = await _workerUi.GetWorkerByIdUi();
            if (workerId <= 0)
            {
                DisplayService.DisplayInfo("Operation cancelled.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            // Create shift using UI
            var shift = await _shiftUi.CreateShiftUi(workerId);

            // Convert to DTO
            var shiftDto = new Models.Dtos.ShiftApiRequestDto
            {
                WorkerId = shift.WorkerId,
                LocationId = shift.LocationId,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            };

            // Create via controller service
            var result = await _controllerService.CreateShiftAsync(shiftDto);

            if (result.IsSuccess)
            {
                DisplayService.DisplaySuccess(result.Message);
                if (result.Data != null)
                {
                    DisplayService.DisplayInfo($"Shift ID: {result.Data.ShiftId}");
                }
            }
            else
            {
                DisplayService.DisplayError(result.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating shift");
            DisplayService.DisplayError($"Failed to create shift: {ex.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task UpdateShiftAsync()
    {
        DisplayService.DisplayHeader("Update Shift");

        var shiftId = await _shiftUi.GetShiftByIdUi();
        if (shiftId <= 0)
        {
            DisplayService.DisplayError("No shift selected.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Get the current shift data
        var getResult = await _controllerService.GetShiftByIdAsync(shiftId);
        if (!getResult.IsSuccess)
        {
            DisplayService.DisplayError($"Failed to retrieve shift: {getResult.Message}");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        if (getResult.Data == null)
        {
            DisplayService.DisplayError("Shift not found.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Get updated data from user
        var updatedShift = await _shiftUi.UpdateShiftUi(getResult.Data);
        if (updatedShift == null)
        {
            DisplayService.DisplayInfo("Update cancelled.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Map to DTO
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = updatedShift.WorkerId,
            StartTime = updatedShift.StartTime,
            EndTime = updatedShift.EndTime,
            LocationId = updatedShift.LocationId
        };

        // Update the shift
        var updateResult = await _controllerService.UpdateShiftAsync(shiftId, shiftDto);
        if (updateResult.IsSuccess && updateResult.Data != null)
        {
            DisplayService.DisplaySuccess("Shift updated successfully!");
            _shiftUi.DisplayShiftsTable([updateResult.Data]);
        }
        else
        {
            DisplayService.DisplayError($"Failed to update shift: {updateResult.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task DeleteShiftAsync()
    {
        DisplayService.DisplayHeader("Delete Shift", "red");

        var shiftId = await _shiftUi.GetShiftByIdUi();
        if (shiftId <= 0)
        {
            DisplayService.DisplayError("No shift selected.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Get the shift details to display before confirmation
        var getResult = await _controllerService.GetShiftByIdAsync(shiftId);
        if (!getResult.IsSuccess || getResult.Data == null)
        {
            DisplayService.DisplayError($"Failed to retrieve shift details: {getResult.Message}");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        var shiftToDelete = getResult.Data;

        // Display shift details in a panel
        DisplayService.DisplayInfo("Shift to be deleted:");
        await _shiftDisplayService.DisplayShiftDetailsAsync(shiftToDelete);

        // Confirm deletion
        var confirm = await InputService.GetConfirmationAsync("Are you sure you want to delete this shift?");
        if (!confirm)
        {
            DisplayService.DisplayInfo("Deletion cancelled.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Delete the shift
        var deleteResult = await _controllerService.DeleteShiftAsync(shiftId);
        if (deleteResult.IsSuccess)
        {
            DisplayService.DisplaySuccess("Shift deleted successfully!");
        }
        else
        {
            DisplayService.DisplayError($"Failed to delete shift: {deleteResult.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task FilterShiftsAsync()
    {
        DisplayService.DisplayHeader("Filter Shifts", "blue");

        // Get filter options from user
        var filterOptions = await _shiftUi.FilterShiftsUi();
        if (filterOptions == null)
        {
            DisplayService.DisplayInfo("Filtering cancelled.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Filter shifts
        var filterResult = await _controllerService.FilterShiftsAsync(filterOptions);
        if (filterResult.IsSuccess && filterResult.Data != null && filterResult.Data.Any())
        {
            _shiftUi.DisplayShiftsTable(filterResult.Data);
        }
        else if (filterResult.IsSuccess && (!filterResult.Data?.Any() ?? true))
        {
            DisplayService.DisplayInfo("No shifts found matching the filter criteria.");
        }
        else
        {
            DisplayService.DisplayError($"Failed to filter shifts: {filterResult.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task HandleShiftSelection(List<Shift> currentPageShifts, int pageNumber, int pageSize, int totalCount)
    {
        try
        {
            if (currentPageShifts == null || !currentPageShifts.Any())
            {
                DisplayService.DisplayError("No shifts available to select from.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            DisplayService.DisplayInfo($"Enter the index number (1-{totalCount}) of the shift you want to view:");
            var globalIndex = await InputService.GetIntegerInputAsync("Index", 1, totalCount);

            // Calculate which page the selected index belongs to
            // Global index is 1-based, so we need to convert to 0-based for calculations
            var zeroBasedIndex = globalIndex - 1;
            var targetPage = (zeroBasedIndex / pageSize) + 1;
            var indexInPage = zeroBasedIndex % pageSize;

            List<Shift> targetPageShifts;

            if (targetPage == pageNumber)
            {
                // Same page, use current data
                targetPageShifts = currentPageShifts;
            }
            else
            {
                // Load the target page
                DisplayService.DisplayInfo($"Loading page {targetPage}...");
                var pageResult = await _controllerService.GetAllShiftsAsync(targetPage, pageSize);

                if (!pageResult.IsSuccess || pageResult.Data == null || pageResult.Data.Data == null || !pageResult.Data.Data.Any())
                {
                    DisplayService.DisplayError("Failed to load the requested page.");
                    await InputService.WaitForKeyPressAsync();
                    return;
                }

                targetPageShifts = pageResult.Data.Data;
            }

            // Validate the index is within the target page bounds
            if (indexInPage >= targetPageShifts.Count)
            {
                DisplayService.DisplayError("Invalid index for the selected page.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            var selectedShift = targetPageShifts[indexInPage];

            // Display the selected shift details
            await _shiftDisplayService.DisplayShiftDetailsAsync(selectedShift);

            await InputService.WaitForKeyPressAsync();
        }
        catch (Exception ex)
        {
            DisplayService.DisplayError($"Error selecting shift: {ex.Message}");
            await InputService.WaitForKeyPressAsync();
        }
    }

    private async Task HandleOperationAsync<T>(
        string operationName,
        Func<Task<OperationResult<T>>> operation,
        Action<T>? onSuccess = null,
        string headerColor = "green")
    {
        DisplayService.DisplayHeader(operationName, headerColor);

        try
        {
            var result = await operation();

            if (result.IsSuccess)
            {
                DisplayService.DisplaySuccess(result.Message);
                onSuccess?.Invoke(result.Data!);
            }
            else
            {
                DisplayService.DisplayError(result.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in {OperationName}", operationName);
            DisplayService.DisplayError($"Failed to {operationName.ToLower()}: {ex.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }
}
