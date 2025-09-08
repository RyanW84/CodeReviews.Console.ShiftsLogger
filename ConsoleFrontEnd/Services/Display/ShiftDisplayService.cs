using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Services.Business;
using ConsoleFrontEnd.Services.Common;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.Services.Display;

/// <summary>
/// Implementation of shift display operations
/// Handles all Spectre.Console table creation and display logic
/// </summary>
public class ShiftDisplayService : IShiftDisplayService
{
    private readonly IConsoleDisplayService _displayService;
    private readonly ILogger<ShiftDisplayService> _logger;

    public ShiftDisplayService(
        IConsoleDisplayService displayService,
        ILogger<ShiftDisplayService> logger)
    {
        _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DisplayShiftsTableWithPaginationAsync(List<Shift> shifts, int pageNumber, int pageSize, int totalCount)
    {
        try
        {
            // Prepare display data (this could be injected as a service in the future)
            var displayData = PrepareShiftDisplayData(shifts, pageNumber, pageSize, totalCount);

            // Create and display the table
            var table = CreateShiftsTable(displayData.Shifts);
            AnsiConsole.Write(table);

            // Show pagination info
            if (displayData.HasMultiplePages)
            {
                _displayService.DisplayInfo($"Page {displayData.PageNumber} of {displayData.TotalPages} (Total: {displayData.TotalCount} shifts)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying shifts table");
            _displayService.DisplayError("Failed to display shifts table");
        }
    }

    public async Task DisplayShiftDetailsAsync(Shift shift)
    {
        try
        {
            _displayService.DisplayHeader("Shift Details", "green");

            var table = new Table();
            table.AddColumn("[bold]Property[/]");
            table.AddColumn("[bold]Value[/]");

            table.AddRow("Worker", shift.Worker?.Name ?? "Unknown");
            table.AddRow("Location", shift.Location?.Name ?? "Unknown");
            table.AddRow("Start Time", shift.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
            table.AddRow("End Time", shift.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
            table.AddRow("Duration", shift.Duration.ToString(@"hh\:mm\:ss"));

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying shift details for shift {ShiftId}", shift.ShiftId);
            _displayService.DisplayError("Failed to display shift details");
        }
    }

    public Table CreateShiftsTable(IEnumerable<ShiftDisplayItem> displayItems)
    {
        var table = new Table();
        table.AddColumn("[bold]Index[/]");
        table.AddColumn("[bold]Worker[/]");
        table.AddColumn("[bold]Location[/]");
        table.AddColumn("[bold]Start Time[/]");
        table.AddColumn("[bold]End Time[/]");
        table.AddColumn("[bold]Duration[/]");

        foreach (var item in displayItems)
        {
            table.AddRow(
                item.DisplayIndex.ToString(),
                item.Shift.Worker?.Name ?? "Unknown",
                item.Shift.Location?.Name ?? "Unknown",
                item.FormattedStartTime,
                item.FormattedEndTime,
                item.FormattedDuration
            );
        }

        return table;
    }

    private ShiftDisplayData PrepareShiftDisplayData(List<Shift> shifts, int pageNumber, int pageSize, int totalCount)
    {
        var displayData = new ShiftDisplayData
        {
            PageNumber = pageNumber,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        // Calculate the starting index for this page
        int startIndex = (pageNumber - 1) * pageSize + 1;

        for (int i = 0; i < shifts.Count; i++)
        {
            var shift = shifts[i];
            var displayItem = new ShiftDisplayItem
            {
                DisplayIndex = startIndex + i,
                Shift = shift,
                FormattedDuration = shift.Duration.ToString(@"hh\:mm"),
                FormattedStartTime = shift.StartTime.ToString("yyyy-MM-dd HH:mm"),
                FormattedEndTime = shift.EndTime.ToString("yyyy-MM-dd HH:mm")
            };

            displayData.Shifts.Add(displayItem);
        }

        return displayData;
    }
}
