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
    /// <summary>
    /// Interactive validation and correction for shift creation/update using helper to eliminate duplication
    /// </summary>
    private async Task<ShiftApiRequestDto> GetValidatedShiftInputAsync(
        ShiftApiRequestDto initial,
        ShiftApiRequestDto? existing = null
    )
    {
        var dto = new ShiftApiRequestDto
        {
            WorkerId = initial.WorkerId,
            LocationId = initial.LocationId,
            StartTime = initial.StartTime,
            EndTime = initial.EndTime,
        };

        // If we're updating (existing provided) prompt the user for each field up-front
        if (existing != null)
        {
            dto.WorkerId = await _shiftInputHelper.SelectWorkerAsync(existing.WorkerId, true);
            dto.LocationId = await _shiftInputHelper.SelectLocationAsync(existing.LocationId, true);
            dto.StartTime = await _shiftInputHelper.GetDateTimeInputAsync(
                "Start Time",
                existing.StartTime,
                true
            );
            dto.EndTime = await _shiftInputHelper.GetDateTimeInputAsync("End Time", existing.EndTime, true);
        }

        // Validation loop
        while (true)
        {
            var errors = ConsoleFrontEnd.Services.Validation.ShiftValidation.Validate(dto);
            if (errors.Count == 0)
                return dto;

            DisplayService.DisplayError("Validation failed:");
            foreach (var error in errors)
                DisplayService.DisplayError(error);

            // For each invalid field, prompt for correction using the helper
            foreach (var error in errors)
            {
                if (error.Contains("WorkerId"))
                {
                    dto.WorkerId = await _shiftInputHelper.SelectWorkerAsync(
                        existing?.WorkerId,
                        existing != null
                    );
                }
                else if (error.Contains("LocationId"))
                {
                    dto.LocationId = await _shiftInputHelper.SelectLocationAsync(
                        existing?.LocationId,
                        existing != null
                    );
                }
                else if (error.Contains("Start time"))
                {
                    dto.StartTime = await _shiftInputHelper.GetDateTimeInputAsync(
                        "Start Time",
                        existing?.StartTime,
                        existing != null
                    );
                }
                else if (error.Contains("End time"))
                {
                    dto.EndTime = await _shiftInputHelper.GetDateTimeInputAsync(
                        "End Time",
                        existing?.EndTime,
                        existing != null
                    );
                }
                else
                {
                    // For any other field, prompt for correction
                    var fieldName = error.Split(' ')[0];
                    var currentValue =
                        existing?.GetType().GetProperty(fieldName)?.GetValue(existing)?.ToString()
                        ?? "";
                    var prompt =
                        existing != null
                            ? $"Enter {fieldName} (current: {currentValue}, press Enter to keep):"
                            : $"Enter {fieldName}:";
                    var input = AnsiConsole.Ask<string>(prompt, "");
                    if (string.IsNullOrWhiteSpace(input) && existing != null)
                    {
                        // Keep current value
                        var prop = dto.GetType().GetProperty(fieldName);
                        if (prop != null && existing != null)
                            prop.SetValue(dto, prop.GetValue(existing));
                    }
                    else
                    {
                        var prop = dto.GetType().GetProperty(fieldName);
                        if (prop != null)
                            prop.SetValue(dto, input);
                    }
                }
            }
        }
    }

    // Use inherited DisplayService, InputService, NavigationService and Logger from BaseMenu
    // Private fields for services specific to this menu (use underscore names because methods reference them)
    private readonly IShiftService _shiftService;
    private readonly IWorkerService _workerService;
    private readonly ILocationService _locationService;
    private readonly IShiftUi _shiftUi;
    private readonly ShiftInputHelper _shiftInputHelper;
    private readonly Dictionary<string, Func<Task<bool>>> MenuActions;

    public ShiftMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<ShiftMenu> logger,
        IShiftService shiftService,
        IWorkerService workerService,
        ILocationService locationService,
        IShiftUi shiftUi,
        ShiftInputHelper shiftInputHelper
    )
        : base(displayService, inputService, navigationService, logger)
    {
        // base constructor sets DisplayService, InputService, NavigationService and Logger
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
        _locationService =
            locationService ?? throw new ArgumentNullException(nameof(locationService));
        _shiftUi = shiftUi ?? throw new ArgumentNullException(nameof(shiftUi));
        _shiftInputHelper =
            shiftInputHelper ?? throw new ArgumentNullException(nameof(shiftInputHelper));

        MenuActions = new Dictionary<string, Func<Task<bool>>>
        {
            ["View All Shifts"] = async () => { await ViewAllShiftsAsync(); return false; },
            ["View Shift by ID"] = async () => { await ViewShiftByIdAsync(); return false; },
            ["Create New Shift"] = async () => { await CreateShiftAsync(); return false; },
            ["Update Shift"] = async () => { await UpdateShiftAsync(); return false; },
            ["Delete Shift"] = async () => { await DeleteShiftAsync(); return false; },
            ["Filter Shifts"] = async () => { await FilterShiftsAsync(); return false; },
            ["View Shifts by Worker"] = async () => { await ViewShiftsByWorkerAsync(); return false; },
            ["View Shifts by Date Range"] = async () => { await ViewShiftsByDateRangeAsync(); return false; },
            ["Back to Main Menu"] = () => Task.FromResult(true)
        };
    }

    public override string Title => "Shift Management";
    public override string Context => "Shift Management";

    private async Task ViewShiftsByWorkerAsync()
    {
        DisplayService.DisplayHeader("Shifts by Worker", "blue");
        var workersResponse = await _workerService.GetAllWorkersAsync().ConfigureAwait(false);
        if (
            workersResponse.RequestFailed
            || workersResponse.Data == null
            || !workersResponse.Data.Any()
        )
        {
            DisplayService.DisplayError(workersResponse.Message ?? "No workers found.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        var workerChoices = workersResponse
            .Data.Select((w, index) => $"{index + 1}. {w.Name}")
            .ToArray();
        var selectedWorkerChoice = await InputService.GetMenuChoiceAsync("Select Worker:", workerChoices);
        var workerCount = UiHelper.ExtractCountFromChoice(selectedWorkerChoice);
        var workerId = workersResponse.Data[workerCount - 1].WorkerId;
        var filter = new ShiftFilterOptions { WorkerId = workerId };
        var response = await _shiftService.GetShiftsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No shifts found for selected worker.");
        }
        else
        {
            _shiftUi.DisplayShiftsTable(response.Data);
            DisplayService.DisplaySuccess($"Total shifts: {response.TotalCount}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task ViewShiftsByDateRangeAsync()
    {
        DisplayService.DisplayHeader("Shifts by Date Range", "blue");

        var startDate = await _shiftInputHelper.GetDateTimeInputAsync("Enter start date");
        var endDate = await _shiftInputHelper.GetDateTimeInputAsync("Enter end date");

        if (endDate <= startDate)
        {
            DisplayService.DisplayError("End date must be after start date.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        var filter = new ShiftFilterOptions { StartTime = startDate, EndTime = endDate };
        var response = await _shiftService.GetShiftsByFilterAsync(filter);

        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No shifts found in date range.");
        }
        else
        {
            _shiftUi.DisplayShiftsTable(response.Data);
            DisplayService.DisplaySuccess($"Found {response.Data.Count()} shifts in date range");
        }

        await InputService.WaitForKeyPressAsync();
    }

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
                "View Shifts by Worker",
                "View Shifts by Date Range",
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

        // Get available workers
        var workersResponse = await _workerService.GetAllWorkersAsync();
        if (workersResponse.RequestFailed)
        {
            switch (workersResponse.ResponseCode)
            {
                case HttpStatusCode.NotFound:
                    DisplayService.DisplayError("No workers found (404).");
                    break;
                case HttpStatusCode.BadRequest:
                    DisplayService.DisplayError("Bad request (400) while retrieving workers.");
                    break;
                case HttpStatusCode.InternalServerError:
                    DisplayService.DisplayError("Server error (500) while retrieving workers.");
                    break;
                case HttpStatusCode.Unauthorized:
                    DisplayService.DisplayError("Unauthorized (401) while retrieving workers.");
                    break;
                case HttpStatusCode.Forbidden:
                    DisplayService.DisplayError("Forbidden (403) while retrieving workers.");
                    break;
                case HttpStatusCode.Conflict:
                    DisplayService.DisplayError("Conflict (409) while retrieving workers.");
                    break;
                case HttpStatusCode.RequestTimeout:
                    DisplayService.DisplayError("Request Timeout (408) while retrieving workers.");
                    break;
                case (HttpStatusCode)422:
                    DisplayService.DisplayError(
                        "Unprocessable Entity (422) while retrieving workers."
                    );
                    break;
                default:
                    DisplayService.DisplayError(
                        $"Failed to retrieve workers: {workersResponse.Message}"
                    );
                    break;
            }

            await InputService.WaitForKeyPressAsync();
            return;
        }

        if (workersResponse.Data == null || !workersResponse.Data.Any())
        {
            DisplayService.DisplayError("No workers found (404). Please create workers first.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Get available locations
        var locationsResponse = await _locationService.GetAllLocationsAsync();
        if (locationsResponse.RequestFailed)
        {
            switch (locationsResponse.ResponseCode)
            {
                case HttpStatusCode.NotFound:
                    DisplayService.DisplayError("No locations found (404).");
                    break;
                case HttpStatusCode.BadRequest:
                    DisplayService.DisplayError("Bad request (400) while retrieving locations.");
                    break;
                case HttpStatusCode.InternalServerError:
                    DisplayService.DisplayError("Server error (500) while retrieving locations.");
                    break;
                case HttpStatusCode.Unauthorized:
                    DisplayService.DisplayError("Unauthorized (401) while retrieving locations.");
                    break;
                case HttpStatusCode.Forbidden:
                    DisplayService.DisplayError("Forbidden (403) while retrieving locations.");
                    break;
                case HttpStatusCode.Conflict:
                    DisplayService.DisplayError("Conflict (409) while retrieving locations.");
                    break;
                case HttpStatusCode.RequestTimeout:
                    DisplayService.DisplayError(
                        "Request Timeout (408) while retrieving locations."
                    );
                    break;
                case (HttpStatusCode)422:
                    DisplayService.DisplayError(
                        "Unprocessable Entity (422) while retrieving locations."
                    );
                    break;
                default:
                    DisplayService.DisplayError(
                        $"Failed to retrieve locations: {locationsResponse.Message}"
                    );
                    break;
            }

            await InputService.WaitForKeyPressAsync();
            return;
        }

        if (locationsResponse.Data == null || !locationsResponse.Data.Any())
        {
            DisplayService.DisplayError("No locations found (404). Please create locations first.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        try
        {
            // Select worker with validation
            var workerChoices = workersResponse
                .Data.Select((w, index) => $"{index + 1}. {w.Name}")
                .ToArray();
            var selectedWorkerChoice = await InputService.GetMenuChoiceAsync("Select Worker:", workerChoices);
            var workerCount = UiHelper.ExtractCountFromChoice(selectedWorkerChoice);
            var workerId = workersResponse.Data[workerCount - 1].WorkerId;
            if (workerId <= 0)
            {
                DisplayService.DisplayError("Invalid worker ID.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            // Select location with validation
            var locationChoices = locationsResponse
                .Data.Select((l, index) => $"{index + 1}. {l.Name}")
                .ToArray();
            var selectedLocationChoice = await InputService.GetMenuChoiceAsync(
                "Select Location:",
                locationChoices
            );
            var locationCount = UiHelper.ExtractCountFromChoice(selectedLocationChoice);
            var locationId = locationsResponse.Data[locationCount - 1].LocationId;
            if (locationId <= 0)
            {
                DisplayService.DisplayError("Invalid location ID.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            // Get shift times with validation
            var startTime = ConsoleFrontEnd.MenuSystem.InputValidator.GetFlexibleDateTime(
                "Enter Start Time (dd/MM/yyyy HH:mm):"
            );
            var endTime = ConsoleFrontEnd.MenuSystem.InputValidator.GetFlexibleDateTime(
                "Enter End Time (dd/MM/yyyy HH:mm):",
                minDate: startTime
            );

            if (endTime <= startTime)
            {
                DisplayService.DisplayError("End time must be after start time.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            // Create shift object
            var newShift = new Shift
            {
                WorkerId = workerId,
                LocationId = locationId,
                StartTime = new DateTimeOffset(startTime),
                EndTime = new DateTimeOffset(endTime),
            };

            // Call the API to create the shift
            var createResponse = await _shiftService.CreateShiftAsync(newShift);
            if (createResponse.RequestFailed)
            {
                var errorDetails =
                    $"Error creating shift.\nStatus: {(int)createResponse.ResponseCode} {createResponse.ResponseCode}\nMessage: {createResponse.Message}";
                if (!string.IsNullOrWhiteSpace(createResponse.Message))
                {
                    errorDetails += $"\nDetails: {createResponse.Message}";
                }
                DisplayService.DisplayError(errorDetails);
                await InputService.WaitForKeyPressAsync();
                return;
            }

            DisplayService.DisplaySuccess("Shift created successfully!");
            DisplayService.DisplayInfo(
                $"Worker: {workersResponse.Data.First(w => w.WorkerId == workerId).Name}"
            );
            DisplayService.DisplayInfo(
                $"Location: {locationsResponse.Data.First(l => l.LocationId == locationId).Name}"
            );
            DisplayService.DisplayInfo($"Start: {startTime}");
            DisplayService.DisplayInfo($"End: {endTime}");
            DisplayService.DisplayInfo(
                $"Duration: {(endTime - startTime).TotalHours:F1} hours ({(endTime - startTime).TotalMinutes:F0} minutes)"
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating shift");
            DisplayService.DisplayError($"Failed to create shift: {ex.Message}");
            await InputService.WaitForKeyPressAsync();
        }
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

        // Get the current shift details
        var shiftResponse = await _shiftService.GetShiftByIdAsync(shiftId);
        if (shiftResponse.RequestFailed || shiftResponse.Data == null)
        {
            DisplayService.DisplayError(
                shiftResponse.Message ?? "Failed to retrieve shift details."
            );
            await InputService.WaitForKeyPressAsync();
            return;
        }

        var shift = shiftResponse.Data;

        // Use the interactive validator to allow keeping current values or selecting new ones
        var existingDto = new ShiftApiRequestDto
        {
            WorkerId = shift.WorkerId,
            LocationId = shift.LocationId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
        };

        var initialDto = new ShiftApiRequestDto
        {
            WorkerId = shift.WorkerId,
            LocationId = shift.LocationId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
        };

        var validated = await GetValidatedShiftInputAsync(initialDto, existingDto);

        var updatedShift = new Shift
        {
            ShiftId = shift.ShiftId,
            WorkerId = validated.WorkerId,
            LocationId = validated.LocationId,
            StartTime = validated.StartTime,
            EndTime = validated.EndTime,
        };
        var response = await _shiftService.UpdateShiftAsync(shiftId, updatedShift);
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "Failed to update shift.");
        }
        else
        {
            DisplayService.DisplaySuccess("Shift updated successfully.");
            _shiftUi.DisplayShiftsTable([response.Data]);
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

        // Get the shift details for confirmation
        var shiftResponse = await _shiftService.GetShiftByIdAsync(shiftId);
        if (shiftResponse.RequestFailed || shiftResponse.Data == null)
        {
            DisplayService.DisplayError(
                shiftResponse.Message ?? "Failed to retrieve shift details."
            );
            await InputService.WaitForKeyPressAsync();
            return;
        }

        var shift = shiftResponse.Data;
        if (
            InputService.GetConfirmation(
                $"Are you sure you want to delete the shift from {shift.StartTime:dd/MM/yyyy HH:mm} to {shift.EndTime:dd/MM/yyyy HH:mm}?"
            )
        )
        {
            var response = await _shiftService.DeleteShiftAsync(shiftId);
            if (response.RequestFailed)
                DisplayService.DisplayError(response.Message ?? "Failed to delete shift.");
            else
                DisplayService.DisplaySuccess(
                    response.Message ?? "Shift deleted successfully."
                );
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task FilterShiftsAsync()
    {
        DisplayService.DisplayHeader("Filter Shifts", "blue");

        // Decide whether to filter by worker
        int? workerId = null;
        var filterByWorker = await InputService.GetMenuChoiceAsync("Filter by worker?", "No", "Yes");
        if (filterByWorker == "Yes")
        {
            workerId = await _shiftInputHelper.SelectWorkerAsync(null, false).ConfigureAwait(false);
            if (workerId <= 0)
            {
                DisplayService.DisplayError("No worker selected.");
                await InputService.WaitForKeyPressAsync();
                return;
            }
        }

        // Decide whether to filter by location
        int? locationId = null;
        var filterByLocation = await InputService.GetMenuChoiceAsync("Filter by location?", "No", "Yes");
        if (filterByLocation == "Yes")
        {
            locationId = await _shiftInputHelper
                .SelectLocationAsync(null, false)
                .ConfigureAwait(false);
            if (locationId <= 0)
            {
                DisplayService.DisplayError("No location selected.");
                await InputService.WaitForKeyPressAsync();
                return;
            }
        }

        // Date filters
        DateTime? startDate = null;
        DateTime? endDate = null;
        var wantDates = await InputService.GetMenuChoiceAsync("Filter by date range?", "No", "Yes");
        if (wantDates == "Yes")
        {
            startDate = (await _shiftInputHelper.GetDateTimeInputAsync("Start Date")).DateTime;
            endDate = (await _shiftInputHelper.GetDateTimeInputAsync("End Date")).DateTime;
            if (endDate <= startDate)
            {
                DisplayService.DisplayError("End date must be after start date.");
                await InputService.WaitForKeyPressAsync();
                return;
            }
        }

        // Duration filters
        int? minDurationMinutes = null;
        int? maxDurationMinutes = null;
        var wantDuration = await InputService.GetMenuChoiceAsync("Filter by duration?", "No", "Yes");
        if (wantDuration == "Yes")
        {
            var minDurationInput = AnsiConsole.Ask<string>(
                "Minimum duration in minutes (press Enter to skip):",
                ""
            );
            if (
                !string.IsNullOrWhiteSpace(minDurationInput)
                && int.TryParse(minDurationInput, out var minDuration)
                && minDuration > 0
            )
            {
                minDurationMinutes = minDuration;
            }

            var maxDurationInput = AnsiConsole.Ask<string>(
                "Maximum duration in minutes (press Enter to skip):",
                ""
            );
            if (
                !string.IsNullOrWhiteSpace(maxDurationInput)
                && int.TryParse(maxDurationInput, out var maxDuration)
                && maxDuration > 0
            )
            {
                maxDurationMinutes = maxDuration;
            }

            if (
                minDurationMinutes.HasValue
                && maxDurationMinutes.HasValue
                && minDurationMinutes > maxDurationMinutes
            )
            {
                DisplayService.DisplayError(
                    "Minimum duration cannot be greater than maximum duration."
                );
                await InputService.WaitForKeyPressAsync();
                return;
            }
        }

        var apiFilter = new ShiftFilterOptions
        {
            WorkerId = workerId,
            LocationId = locationId,
            StartDate = startDate,
            EndDate = endDate,
            MinDurationMinutes = minDurationMinutes,
            MaxDurationMinutes = maxDurationMinutes,
        };

        var response = await _shiftService.GetShiftsByFilterAsync(apiFilter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No shifts found matching filter.");
        }
        else
        {
            _shiftUi.DisplayShiftsTable(response.Data);
            DisplayService.DisplaySuccess($"Total filtered shifts: {response.TotalCount}");
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
