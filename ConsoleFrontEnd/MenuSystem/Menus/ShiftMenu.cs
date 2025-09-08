using System.Net;
using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
///     Shift menu implementation following Single Responsibility Principle
///     Handles shift-specific operations
/// </summary>
public class ShiftMenu : BaseMenu
{
    // Use inherited DisplayService, InputService, NavigationService and Logger from BaseMenu
    // Private fields for services specific to this menu (use underscore names because methods reference them)
    private readonly IShiftService _shiftService;
    private readonly IShiftUi _shiftUi;
    private readonly ConsoleFrontEnd.Services.Business.IShiftOrchestrationService _orchestrationService;
    private readonly Dictionary<string, Func<Task<bool>>> MenuActions;

    public ShiftMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<ShiftMenu> logger,
        IShiftService shiftService,
        IShiftUi shiftUi,
        ConsoleFrontEnd.Services.Business.IShiftOrchestrationService orchestrationService
    )
        : base(displayService, inputService, navigationService, logger)
    {
        // base constructor sets DisplayService, InputService, NavigationService and Logger
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _shiftUi = shiftUi ?? throw new ArgumentNullException(nameof(shiftUi));
        _orchestrationService = orchestrationService ?? throw new ArgumentNullException(nameof(orchestrationService));

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
            int pageSize = 10;
            bool continuePaging = true;

            while (continuePaging)
            {
                DisplayService.DisplayHeader($"All Shifts - Page {pageNumber}", "blue");

                var response = await _shiftService.GetAllShiftsAsync(pageNumber, pageSize);

                if (response.RequestFailed)
                {
                    DisplayService.DisplayError($"Failed to retrieve shifts: {response.Message}");
                    return;
                }

                if (response.Data == null || response.Data.Count == 0)
                {
                    DisplayService.DisplayError("No shifts found.");
                    return;
                }

                // Calculate starting index for this page
                int startIndex = (pageNumber - 1) * pageSize + 1;

                // Display shifts with proper pagination numbering
                DisplayShiftsTableWithPagination(response.Data, pageNumber, pageSize, response.TotalCount);

                // Ask user what they want to do
                var options = new List<string> { "Select Shift", "Back to Menu" };
                if (pageNumber > 1) options.Insert(0, "Previous Page");
                if (pageNumber * pageSize < response.TotalCount) options.Insert(options.Count - 1, "Next Page");

                var choice = await InputService.GetMenuChoiceAsync("What would you like to do?", options.ToArray());

                switch (choice)
                {
                    case "Next Page":
                        pageNumber++;
                        break;
                    case "Previous Page":
                        pageNumber = Math.Max(1, pageNumber - 1);
                        break;
                    case "Select Shift":
                        await HandleShiftSelection(response.Data, pageNumber, pageSize);
                        break;
                    case "Back to Menu":
                        continuePaging = false;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
        }
    }

    private async Task ViewShiftByIdAsync()
    {
        try
        {
            DisplayService.DisplayHeader("View Shift by ID", "blue");

            var shiftId = await _shiftUi.GetShiftByIdUi();

            if (shiftId <= 0)
            {
                DisplayService.DisplayInfo("Operation cancelled.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            var response = await _shiftService.GetShiftByIdAsync(shiftId);

            if (response.RequestFailed)
            {
                DisplayService.DisplayError($"Failed to retrieve shift: {response.Message}");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            if (response.Data == null)
            {
                DisplayService.DisplayError("Shift not found.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            await DisplayShiftDetails(response.Data);
        }
        catch (Exception ex)
        {
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
            await InputService.WaitForKeyPressAsync();
        }
    }

    private async Task CreateShiftAsync()
    {
        DisplayService.DisplayHeader("Create New Shift", "green");

        try
        {
            var result = await _orchestrationService.CreateShiftWorkflowAsync();

            if (result.IsSuccess)
            {
                DisplayService.DisplaySuccess(result.Message);
                if (result.Data != null)
                {
                    // Display the created shift details
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

        try
        {
            var shiftId = await _shiftUi.GetShiftByIdUi();
            if (shiftId <= 0)
            {
                DisplayService.DisplayError("No shift selected.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            var result = await _orchestrationService.UpdateShiftWorkflowAsync(shiftId);

            if (result.IsSuccess)
            {
                DisplayService.DisplaySuccess(result.Message);
                if (result.Data != null)
                {
                    _shiftUi.DisplayShiftsTable([result.Data]);
                }
            }
            else
            {
                DisplayService.DisplayError(result.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating shift");
            DisplayService.DisplayError($"Failed to update shift: {ex.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task DeleteShiftAsync()
    {
        DisplayService.DisplayHeader("Delete Shift", "red");

        try
        {
            var shiftId = await _shiftUi.GetShiftByIdUi();
            if (shiftId <= 0)
            {
                DisplayService.DisplayError("No shift selected.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            var result = await _orchestrationService.DeleteShiftWorkflowAsync(shiftId);

            if (result.IsSuccess)
            {
                DisplayService.DisplaySuccess(result.Message);
            }
            else
            {
                DisplayService.DisplayError(result.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting shift");
            DisplayService.DisplayError($"Failed to delete shift: {ex.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task FilterShiftsAsync()
    {
        DisplayService.DisplayHeader("Filter Shifts", "blue");

        try
        {
            var result = await _orchestrationService.FilterShiftsWorkflowAsync();

            if (result.IsSuccess)
            {
                DisplayService.DisplaySuccess(result.Message);
                if (result.Data != null && result.Data.Any())
                {
                    _shiftUi.DisplayShiftsTable(result.Data.ToList());
                }
            }
            else
            {
                DisplayService.DisplayError(result.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error filtering shifts");
            DisplayService.DisplayError($"Failed to filter shifts: {ex.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private void DisplayShiftsTableWithPagination(List<Shift> shifts, int pageNumber, int pageSize, int totalCount)
    {
        if (shifts == null || shifts.Count == 0)
        {
            DisplayService.DisplayError("No shifts found.");
            return;
        }

        var table = new Table();
        table.AddColumn("[bold]Index[/]");
        table.AddColumn("[bold]Worker[/]");
        table.AddColumn("[bold]Location[/]");
        table.AddColumn("[bold]Start Time[/]");
        table.AddColumn("[bold]End Time[/]");
        table.AddColumn("[bold]Duration[/]");

        // Calculate the starting index for this page
        int startIndex = (pageNumber - 1) * pageSize + 1;

        for (int i = 0; i < shifts.Count; i++)
        {
            var shift = shifts[i];
            // Use the Duration property which is already computed
            var duration = shift.Duration.ToString(@"hh\:mm");

            // Use the actual index that accounts for pagination
            int displayIndex = startIndex + i;

            table.AddRow(
                displayIndex.ToString(),
                shift.Worker?.Name ?? "Unknown",
                shift.Location?.Name ?? "Unknown",
                shift.StartTime.ToString("yyyy-MM-dd HH:mm"),
                shift.EndTime.ToString("yyyy-MM-dd HH:mm"),
                duration
            );
        }

        AnsiConsole.Write(table);

        // Show pagination info
        if (totalCount > pageSize)
        {
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            DisplayService.DisplayInfo($"Page {pageNumber} of {totalPages} (Total: {totalCount} shifts)");
        }
    }

    private async Task HandleShiftSelection(List<Shift> currentPageShifts, int pageNumber, int pageSize)
    {
        try
        {
            DisplayService.DisplayInfo("Enter the Index number of the shift you want to view:");
            var indexInput = await InputService.GetTextInputAsync("Index");

            if (!int.TryParse(indexInput, out int selectedIndex) || selectedIndex < 1)
            {
                DisplayService.DisplayError("Invalid index. Please enter a valid number.");
                return;
            }

            // Calculate the actual position in the current page
            int startIndex = (pageNumber - 1) * pageSize + 1;
            int endIndex = startIndex + currentPageShifts.Count - 1;

            // Check if the selected index is within the current page range
            if (selectedIndex < startIndex || selectedIndex > endIndex)
            {
                DisplayService.DisplayError($"Index must be between {startIndex} and {endIndex} for this page.");
                return;
            }

            // Convert the global index to local page index
            int localIndex = selectedIndex - startIndex;
            var selectedShift = currentPageShifts[localIndex];

            // Display the selected shift details
            await DisplayShiftDetails(selectedShift);
        }
        catch (Exception ex)
        {
            DisplayService.DisplayError($"Error selecting shift: {ex.Message}");
        }
    }

    private async Task DisplayShiftDetails(Shift shift)
    {
        DisplayService.DisplayHeader("Shift Details", "green");

        var table = new Table();
        table.AddColumn("[bold]Property[/]");
        table.AddColumn("[bold]Value[/]");

        table.AddRow("Worker", shift.Worker?.Name ?? "Unknown");
        table.AddRow("Location", shift.Location?.Name ?? "Unknown");
        table.AddRow("Start Time", shift.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("End Time", shift.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("Duration", shift.Duration.ToString(@"hh\:mm\:ss"));

        AnsiConsole.Write(table);
        await InputService.WaitForKeyPressAsync();
    }
}
