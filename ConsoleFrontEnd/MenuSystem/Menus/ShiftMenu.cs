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
    private const int DefaultPageSize = 10;

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
            int pageNumber = 1;
            bool continuePaging = true;

            while (continuePaging)
            {
                var (success, response) = await LoadShiftsPageAsync(pageNumber);
                if (!success) return;

                await DisplayShiftsPageAsync(response!, pageNumber);

                var choice = await GetUserPaginationChoiceAsync(pageNumber, DefaultPageSize, response!.TotalCount);

                var (paginationSuccess, workflowResult) = await ProcessPaginationChoiceAsync(pageNumber, DefaultPageSize, choice);
                if (!paginationSuccess) return;

                continuePaging = workflowResult!.ShouldContinue;
                pageNumber = workflowResult.NextPageNumber;

                if (workflowResult.ShouldSelectShift && workflowResult.ShiftData != null && workflowResult.ShiftData.Data != null)
                {
                    await HandleShiftSelection(workflowResult.ShiftData.Data, pageNumber, DefaultPageSize);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing all shifts");
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
        }
    }

    private async Task<(bool Success, PaginatedApiResponseDto<List<Shift>>? Response)> LoadShiftsPageAsync(int pageNumber)
    {
        DisplayService.DisplayHeader($"All Shifts - Page {pageNumber}", "blue");

        var result = await _controllerService.GetAllShiftsAsync(pageNumber, DefaultPageSize);

        if (!result.IsSuccess)
        {
            DisplayService.DisplayError(result.Message);
            await InputService.WaitForKeyPressAsync();
            return (false, null);
        }

        var response = result.Data!;
        if (response.Data == null)
        {
            DisplayService.DisplayError("No shift data available.");
            await InputService.WaitForKeyPressAsync();
            return (false, null);
        }

        return (true, response);
    }

    private async Task DisplayShiftsPageAsync(PaginatedApiResponseDto<List<Shift>> response, int pageNumber)
    {
        await _shiftDisplayService.DisplayShiftsTableWithPaginationAsync(
            response.Data!, pageNumber, DefaultPageSize, response.TotalCount);
    }

    private async Task<string> GetUserPaginationChoiceAsync(int pageNumber, int pageSize, int totalCount)
    {
        var options = new List<string> { "Select Shift", "Back to Menu" };
        if (pageNumber > 1) options.Insert(0, "Previous Page");
        if (pageNumber * pageSize < totalCount) options.Insert(options.Count - 1, "Next Page");

        return await InputService.GetMenuChoiceAsync("What would you like to do?", options.ToArray());
    }

    private async Task<(bool Success, PaginationWorkflowResult? Result)> ProcessPaginationChoiceAsync(int pageNumber, int pageSize, string choice)
    {
        try
        {
            switch (choice)
            {
                case "Next Page":
                    var nextPage = pageNumber + 1;
                    var nextPageResult = await _controllerService.GetAllShiftsAsync(nextPage, pageSize);

                    if (!nextPageResult.IsSuccess)
                    {
                        DisplayService.DisplayError(nextPageResult.Message);
                        await InputService.WaitForKeyPressAsync();
                        return (false, null);
                    }

                    return (true, new PaginationWorkflowResult
                    {
                        ShouldContinue = true,
                        NextPageNumber = nextPage,
                        ShouldSelectShift = false,
                        ShiftData = nextPageResult.Data
                    });

                case "Previous Page":
                    var prevPage = Math.Max(1, pageNumber - 1);
                    var prevPageResult = await _controllerService.GetAllShiftsAsync(prevPage, pageSize);

                    if (!prevPageResult.IsSuccess)
                    {
                        DisplayService.DisplayError(prevPageResult.Message);
                        await InputService.WaitForKeyPressAsync();
                        return (false, null);
                    }

                    return (true, new PaginationWorkflowResult
                    {
                        ShouldContinue = true,
                        NextPageNumber = prevPage,
                        ShouldSelectShift = false,
                        ShiftData = prevPageResult.Data
                    });

                case "Select Shift":
                    var currentPageResult = await _controllerService.GetAllShiftsAsync(pageNumber, pageSize);

                    if (!currentPageResult.IsSuccess)
                    {
                        DisplayService.DisplayError(currentPageResult.Message);
                        await InputService.WaitForKeyPressAsync();
                        return (false, null);
                    }

                    return (true, new PaginationWorkflowResult
                    {
                        ShouldContinue = true,
                        NextPageNumber = pageNumber,
                        ShouldSelectShift = true,
                        ShiftData = currentPageResult.Data
                    });

                case "Back to Menu":
                    return (true, new PaginationWorkflowResult
                    {
                        ShouldContinue = false,
                        NextPageNumber = pageNumber,
                        ShouldSelectShift = false,
                        ShiftData = null
                    });

                default:
                    DisplayService.DisplayError("Invalid pagination choice");
                    await InputService.WaitForKeyPressAsync();
                    return (false, null);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing pagination choice: {Choice}", choice);
            DisplayService.DisplayError("An error occurred during pagination");
            await InputService.WaitForKeyPressAsync();
            return (false, null);
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

        // Confirm deletion
        var confirm = await InputService.GetConfirmationAsync($"Are you sure you want to delete shift {shiftId}?");
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

    private async Task HandleShiftSelection(List<Shift> currentPageShifts, int pageNumber, int pageSize)
    {
        try
        {
            DisplayService.DisplayInfo("Enter the Index number of the shift you want to view:");
            var indexInput = await InputService.GetTextInputAsync("Index");

            if (!int.TryParse(indexInput, out int selectedIndex) || selectedIndex < 1 || selectedIndex > currentPageShifts.Count)
            {
                DisplayService.DisplayError("Invalid index. Please enter a number between 1 and " + currentPageShifts.Count);
                return;
            }

            var selectedShift = currentPageShifts[selectedIndex - 1];

            // Display the selected shift details
            await _shiftDisplayService.DisplayShiftDetailsAsync(selectedShift);
        }
        catch (Exception ex)
        {
            DisplayService.DisplayError($"Error selecting shift: {ex.Message}");
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
