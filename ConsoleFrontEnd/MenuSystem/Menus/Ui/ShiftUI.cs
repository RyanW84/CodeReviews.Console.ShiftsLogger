using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Threading.Tasks;

namespace ConsoleFrontEnd.MenuSystem;

public class ShiftUI : IShiftUi
{
    private readonly IConsoleDisplayService _display;
    private readonly UiHelper _uiHelper;
    private readonly ShiftInputHelper _shiftInputHelper;
    private readonly ConsoleFrontEnd.Interfaces.IShiftService _shiftService;

    public ShiftUI(IConsoleDisplayService display, ILogger<ShiftUI> logger, ShiftInputHelper shiftInputHelper, ConsoleFrontEnd.Interfaces.IShiftService shiftService)
    {
        _display = display;
        _uiHelper = new UiHelper(display, logger);
        _shiftInputHelper = shiftInputHelper ?? throw new ArgumentNullException(nameof(shiftInputHelper));
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
    }

    public async Task<Shift> CreateShiftUi(int workerId)
    {
        _display.DisplayHeader("Create New Shift");

        var start = _shiftInputHelper.GetDateTimeInput("Start Time");
        var end = _shiftInputHelper.GetDateTimeInput("End Time");
        var locationId = await _shiftInputHelper.SelectLocationAsync(null, false).ConfigureAwait(false);

        return new Shift
        {
            ShiftId = 0, // Will be assigned by service
            WorkerId = workerId,
            LocationId = locationId,
            StartTime = start,
            EndTime = end
        };
    }

    public async Task<Shift> UpdateShiftUi(Shift existingShift)
    {
        _display.DisplayHeader($"Update Shift ID: {existingShift.Id}");

        var start = _shiftInputHelper.GetDateTimeInput("Start Time", existingShift.Start, true);
        var end = _shiftInputHelper.GetDateTimeInput("End Time", existingShift.End, true);
        var locationId = await _shiftInputHelper.SelectLocationAsync(existingShift.LocationId, true).ConfigureAwait(false);

        return new Shift
        {
            ShiftId = existingShift.Id,
            WorkerId = existingShift.WorkerId,
            LocationId = locationId,
            StartTime = start,
            EndTime = end
        };
    }

    public async Task<ShiftFilterOptions> FilterShiftsUi()
    {
        _display.DisplayHeader("Filter Shifts");

        int? workerId = null;
        var filterByWorker = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by worker?")
                .AddChoices(new[] { "No", "Yes" })
        );
        if (filterByWorker == "Yes")
            workerId = await _shiftInputHelper.SelectWorkerAsync(null, false).ConfigureAwait(false);

        int? locationId = null;
        var filterByLocation = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by location?")
                .AddChoices(new[] { "No", "Yes" })
        );
        if (filterByLocation == "Yes")
            locationId = await _shiftInputHelper.SelectLocationAsync(null, false).ConfigureAwait(false);

        DateTime? startDate = null;
        DateTime? endDate = null;
        var wantDates = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by date range?")
                .AddChoices(new[] { "No", "Yes" })
        );
        if (wantDates == "Yes")
        {
            startDate = _shiftInputHelper.GetDateTimeInput("Start Date").DateTime;
            endDate = _shiftInputHelper.GetDateTimeInput("End Date").DateTime;
        }

        int? minDurationMinutes = null;
        int? maxDurationMinutes = null;
        var wantDuration = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by duration?")
                .AddChoices(new[] { "No", "Yes" })
        );
        if (wantDuration == "Yes")
        {
            var minDurationInput = AnsiConsole.Ask<string>("Minimum duration in minutes (press Enter to skip):", "");
            if (!string.IsNullOrWhiteSpace(minDurationInput) && int.TryParse(minDurationInput, out var minDuration) && minDuration > 0)
            {
                minDurationMinutes = minDuration;
            }

            var maxDurationInput = AnsiConsole.Ask<string>("Maximum duration in minutes (press Enter to skip):", "");
            if (!string.IsNullOrWhiteSpace(maxDurationInput) && int.TryParse(maxDurationInput, out var maxDuration) && maxDuration > 0)
            {
                maxDurationMinutes = maxDuration;
            }
        }

        return new ShiftFilterOptions
        {
            WorkerId = workerId,
            LocationId = locationId,
            StartDate = startDate,
            EndDate = endDate,
            MinDurationMinutes = minDurationMinutes,
            MaxDurationMinutes = maxDurationMinutes
        };
    }

    public void DisplayShiftsTable(IEnumerable<Shift> shifts, int startingRowNumber = 1)
    {
        _display.DisplayTable(shifts, "Shifts", startingRowNumber);
    }

    public async Task DisplayShiftsWithPaginationAsync(int initialPageNumber = 1, int pageSize = 10)
    {
        var currentPage = initialPageNumber;

        while (true)
        {
            _display.DisplayHeader($"Shifts (Page {currentPage})", "blue");

            var response = await _shiftService.GetAllShiftsAsync(currentPage, pageSize).ConfigureAwait(false);

            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    _display.DisplayError("No shifts found.");
                    return;
                }
                else
                {
                    _display.DisplayError($"No shifts found on page {currentPage}. Returning to page 1.");
                    currentPage = 1;
                    continue;
                }
            }

            // Calculate starting index for continuous numbering across pages
            int startIndex = (currentPage - 1) * pageSize;

            DisplayShiftsTable(response.Data, startIndex + 1);

            // Display pagination info
            _display.DisplayInfo($"Page {response.PageNumber} of {response.TotalPages} | Total: {response.TotalCount} shifts");
            _display.DisplayInfo($"Showing {response.Data.Count()} of {response.TotalCount} shifts");

            // Create pagination options
            var options = new List<string>();

            if (response.HasPreviousPage)
                options.Add("Previous Page");

            if (response.HasNextPage)
                options.Add("Next Page");

            options.Add("Go to Page");
            options.Add("Change Page Size");
            options.Add("Back to Menu");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action:")
                    .AddChoices(options)
            );

            switch (choice)
            {
                case "Previous Page":
                    currentPage--;
                    break;

                case "Next Page":
                    currentPage++;
                    break;

                case "Go to Page":
                    var pageInput = AnsiConsole.Ask<int>($"Enter page number (1-{response.TotalPages}):");
                    if (pageInput >= 1 && pageInput <= response.TotalPages)
                        currentPage = pageInput;
                    else
                        _display.DisplayError($"Invalid page number. Please enter a number between 1 and {response.TotalPages}.");
                    break;

                case "Change Page Size":
                    var sizeInput = AnsiConsole.Ask<int>("Enter new page size (1-100):");
                    if (sizeInput >= 1 && sizeInput <= 100)
                    {
                        pageSize = sizeInput;
                        currentPage = 1; // Reset to first page
                    }
                    else
                        _display.DisplayError("Invalid page size. Please enter a number between 1 and 100.");
                    break;

                case "Back to Menu":
                    return;
            }
        }
    }

    public async Task<int> GetShiftByIdUi()
    {
        _display.DisplayHeader("Select Shift", "blue");

        var currentPage = 1;
        const int pageSize = 10; // Display 10 items at a time

        while (true)
        {
            var response = await _shiftService.GetAllShiftsAsync(currentPage, pageSize).ConfigureAwait(false);
            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    _uiHelper.DisplayValidationError(response.Message ?? "No shifts available.");
                    // Fallback to manual entry
                    return AnsiConsole.Ask<int>("[green]Enter shift ID:[/]");
                }
                else
                {
                    currentPage = 1;
                    continue;
                }
            }

            // Calculate starting index for continuous numbering across pages
            int startIndex = (currentPage - 1) * pageSize;

            var choices = response.Data
                .Select((s, index) => $"{startIndex + index + 1}. {s.StartTime:dd/MM/yyyy HH:mm} - {s.EndTime:dd/MM/yyyy HH:mm} ({s.Duration.TotalHours:F1}h)")
                .ToList();

            // Add navigation options if there are more pages
            if (response.HasNextPage)
                choices.Add("Next Page...");
            if (response.HasPreviousPage)
                choices.Add("Previous Page...");

            choices.Add("Enter ID Manually");
            choices.Add("Cancel/Return to Menu");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Select Shift (Page {response.PageNumber} of {response.TotalPages}):")
                    .AddChoices(choices)
            );

            if (selected == "Next Page...")
            {
                currentPage++;
                continue;
            }
            else if (selected == "Previous Page...")
            {
                currentPage--;
                continue;
            }
            else if (selected == "Enter ID Manually")
            {
                return AnsiConsole.Ask<int>("[green]Enter shift ID:[/]");
            }
            else if (selected == "Cancel/Return to Menu")
            {
                return -1; // Signal cancellation
            }
            else
            {
                // Extract the count from the selected choice and get the corresponding shift
                var count = UiHelper.ExtractCountFromChoice(selected);
                if (count > 0 && count <= response.Data.Count)
                {
                    return response.Data[count - 1].ShiftId;
                }
                else
                {
                    _display.DisplayError("Invalid selection.");
                    continue;
                }
            }
        }
    }
}