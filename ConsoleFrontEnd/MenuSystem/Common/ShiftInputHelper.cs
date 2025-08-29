using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem.Common;

/// <summary>
/// Dedicated helper for shift input operations to eliminate duplication
/// Follows Single Responsibility Principle
/// </summary>
public class ShiftInputHelper
{
    private readonly IWorkerService _workerService;
    private readonly ILocationService _locationService;
    private readonly IConsoleDisplayService _displayService;
    private readonly ILogger<ShiftInputHelper> _logger;

    public ShiftInputHelper(
        IWorkerService workerService,
        ILocationService locationService,
        IConsoleDisplayService displayService,
        ILogger<ShiftInputHelper> logger)
    {
        _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Select worker with current value handling
    /// </summary>
    public async Task<int> SelectWorkerAsync(int? currentWorkerId, bool allowKeepCurrent = false)
    {
        var workersResponse = await _workerService.GetAllWorkersAsync().ConfigureAwait(false);
        var workers = workersResponse.Data ?? [];

        if (allowKeepCurrent && currentWorkerId.HasValue)
        {
            var currentWorker = workers.FirstOrDefault(w => w.WorkerId == currentWorkerId);
            var currentName = currentWorker?.Name ?? "None";
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Current worker:[/] {currentName}");
            
            List<string> choices = ["(Keep current)"];
            choices.AddRange(workers.Select(w => w.Name));
            
            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Worker:")
                    .AddChoices(choices));

            if (selected == "(Keep current)")
                return currentWorkerId.Value;
            
            var worker = workers.FirstOrDefault(w => w.Name == selected);
            return worker?.WorkerId ?? currentWorkerId.Value;
        }
        else
        {
            var choices = workers.Select(w => w.Name).ToList();
            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Worker:")
                    .AddChoices(choices));

            var worker = workers.FirstOrDefault(w => w.Name == selected);
            return worker?.WorkerId ?? 0;
        }
    }

    /// <summary>
    /// Select location with current value handling
    /// </summary>
    public async Task<int> SelectLocationAsync(int? currentLocationId, bool allowKeepCurrent = false)
    {
        var locationsResponse = await _locationService.GetAllLocationsAsync().ConfigureAwait(false);
        var locations = locationsResponse.Data ?? [];

        if (allowKeepCurrent && currentLocationId.HasValue)
        {
            var currentLocation = locations.FirstOrDefault(l => l.LocationId == currentLocationId);
            var currentName = currentLocation?.Name ?? "None";
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Current location:[/] {currentName}");
            
            List<string> choices = ["(Keep current)"];
            choices.AddRange(locations.Select(l => l.Name));
            
            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Location:")
                    .AddChoices(choices));

            if (selected == "(Keep current)")
                return currentLocationId.Value;
            
            var location = locations.FirstOrDefault(l => l.Name == selected);
            return location?.LocationId ?? currentLocationId.Value;
        }
        else
        {
            var choices = locations.Select(l => l.Name).ToList();
            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Location:")
                    .AddChoices(choices));

            var location = locations.FirstOrDefault(l => l.Name == selected);
            return location?.LocationId ?? 0;
        }
    }

    /// <summary>
    /// Get datetime input with validation and current value handling
    /// </summary>
    public DateTimeOffset GetDateTimeInput(string fieldName, DateTimeOffset? currentValue = null, bool allowKeepCurrent = false)
    {
        while (true)
        {
            var prompt = allowKeepCurrent && currentValue.HasValue
                ? $"Enter {fieldName} (current: {currentValue:dd/MM/yyyy HH:mm}, press Enter to keep):"
                : $"Enter {fieldName} (dd/MM/yyyy HH:mm):";

            var input = AnsiConsole.Ask<string>(prompt, "");
            
            if (string.IsNullOrWhiteSpace(input) && allowKeepCurrent && currentValue.HasValue)
            {
                return currentValue.Value;
            }

            if (DateTime.TryParseExact(input, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var value))
            {
                return new DateTimeOffset(value);
            }
            
            if (DateTime.TryParse(input, out var any))
            {
                return new DateTimeOffset(any);
            }
            
            _displayService.DisplayError("Invalid date format. Please use dd/MM/yyyy HH:mm or dd-MM-yyyy HH:mm");
        }
    }
}
