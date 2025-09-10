using System.Threading.Tasks;
using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Base;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class ShiftUI : BaseEntityUi<Shift, ShiftFilterOptions>, IShiftUi
{
    private readonly IConsoleInputService _input;
    private readonly UiHelper _uiHelper;
    private readonly ShiftInputHelper _shiftInputHelper;
    private readonly ConsoleFrontEnd.Interfaces.IShiftService _shiftService;
    private readonly PaginationHandler _paginationHandler;

    public ShiftUI(
        IConsoleDisplayService display,
        IConsoleInputService input,
        ILogger<ShiftUI> logger,
        ShiftInputHelper shiftInputHelper,
        ConsoleFrontEnd.Interfaces.IShiftService shiftService
    ) : base(display, logger)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _uiHelper = new UiHelper(display, logger);
        _shiftInputHelper = shiftInputHelper ?? throw new ArgumentNullException(nameof(shiftInputHelper));
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _paginationHandler = new PaginationHandler(display, input);
    }

    protected override string EntityName => "Shift";
    protected override string EntityPluralName => "Shifts";

    public async Task<Shift> CreateShiftUi(int workerId)
    {
        DisplayCreateHeader();
        var start = await _shiftInputHelper.GetDateTimeInputAsync("Start Time");
        var end = await _shiftInputHelper.GetDateTimeInputAsync("End Time");
        var locationId = await _shiftInputHelper.SelectLocationAsync(null, false);
        return new Shift
        {
            ShiftId = 0,
            WorkerId = workerId,
            LocationId = locationId,
            StartTime = start,
            EndTime = end,
        };
    }

    public async Task<Shift> UpdateShiftUi(Shift existingShift)
    {
        DisplayUpdateHeader(existingShift.Id.ToString());
        var workerId = await _shiftInputHelper.SelectWorkerAsync(existingShift.WorkerId, true);
        var start = await _shiftInputHelper.GetDateTimeInputAsync("Start Time", existingShift.Start, true);
        var end = await _shiftInputHelper.GetDateTimeInputAsync("End Time", existingShift.End, true);
        var locationId = await _shiftInputHelper.SelectLocationAsync(existingShift.LocationId, true);
        return new Shift
        {
            ShiftId = existingShift.Id,
            WorkerId = workerId,
            LocationId = locationId,
            StartTime = start,
            EndTime = end,
        };
    }

    public async Task<ShiftFilterOptions> FilterShiftsUi()
    {
        DisplayFilterHeader();
        int? workerId = null;
        if (await _input.GetMenuChoiceAsync("Filter by worker?", "No", "Yes") == "Yes")
            workerId = await _shiftInputHelper.SelectWorkerAsync(null, false);

        int? locationId = null;
        if (await _input.GetMenuChoiceAsync("Filter by location?", "No", "Yes") == "Yes")
            locationId = await _shiftInputHelper.SelectLocationAsync(null, false);

        DateTime? startDate = null, endDate = null;
        if (await _input.GetMenuChoiceAsync("Filter by date range?", "No", "Yes") == "Yes")
        {
            startDate = (await _shiftInputHelper.GetDateTimeInputAsync("Start Date")).DateTime;
            endDate = (await _shiftInputHelper.GetDateTimeInputAsync("End Date")).DateTime;
        }

        int? minDuration = null, maxDuration = null;
        if (await _input.GetMenuChoiceAsync("Filter by duration?", "No", "Yes") == "Yes")
        {
            var minInput = await _input.GetTextInputAsync("Minimum duration in minutes (press Enter to skip):", false);
            if (int.TryParse(minInput, out var min) && min > 0) minDuration = min;
            var maxInput = await _input.GetTextInputAsync("Maximum duration in minutes (press Enter to skip):", false);
            if (int.TryParse(maxInput, out var max) && max > 0) maxDuration = max;
        }

        return new ShiftFilterOptions
        {
            WorkerId = workerId,
            LocationId = locationId,
            StartDate = startDate,
            EndDate = endDate,
            MinDurationMinutes = minDuration,
            MaxDurationMinutes = maxDuration,
        };
    }

    public void DisplayShiftsTable(IEnumerable<Shift> shifts, int startingRowNumber = 1)
    {
        _display.DisplayTable(shifts, EntityPluralName, startingRowNumber);
    }

    public async Task DisplayShiftsWithPaginationAsync(int initialPageNumber = 1, int pageSize = 10)
    {
        await _paginationHandler.HandlePaginationAsync(
            async (page, size) => await _shiftService.GetAllShiftsAsync(page, size),
            (response, page, size) => DisplayPage(response, page, size),
            null, // No selection
            null, // No selection handler
            initialPageNumber,
            pageSize
        );
    }

    public async Task<(bool Selected, int ShiftId)> DisplayShiftsWithPaginationAndSelectionAsync(
        int initialPageNumber = 1,
        int pageSize = 10
    )
    {
        var result = await _paginationHandler.HandlePaginationAsync(
            async (page, size) => await _shiftService.GetAllShiftsAsync(page, size),
            (response, page, size) => DisplayPage(response, page, size),
            BuildChoices,
            async (selected, response, page, size) =>
            {
                await Task.CompletedTask;
                if (selected == "Next Page...") return null; // Continue pagination
                else if (selected == "Previous Page...") return null;
                else if (selected == "Enter ID Manually") return await _input.GetIntegerInputAsync("[green]Enter shift ID:[/]");
                else if (selected == "Cancel/Return to Menu") return -1;
                else
                {
                    var choices = BuildChoices(response, page, size);
                    var index = choices.IndexOf(selected);
                    if (index >= 0 && response.Data != null && index < response.Data.Count)
                    {
                        return response.Data[index].ShiftId;
                    }
                }
                return null;
            },
            initialPageNumber,
            pageSize
        );

        return result != null ? (true, (int)result) : (false, -1);
    }

    private void DisplayPage(ApiResponseDto<List<Shift>> response, int currentPage, int pageSize)
    {
        int startIndex = (currentPage - 1) * pageSize;
        if (response.Data != null) DisplayShiftsTable(response.Data, startIndex + 1);
        _display.DisplayInfo($"Page {response.PageNumber} of {response.TotalPages} | Total: {response.TotalCount} shifts");
    }

    public async Task<int> GetShiftByIdUi()
    {
        var result = await _paginationHandler.HandlePaginationAsync(
            async (page, size) => await _shiftService.GetAllShiftsAsync(page, size),
            (response, page, size) => DisplayPage(response, page, size),
            BuildChoices,
            async (selected, response, page, size) =>
            {
                if (selected == "Next Page...") return null; // Continue pagination
                else if (selected == "Previous Page...") return null;
                else if (selected == "Enter ID Manually") return await _input.GetIntegerInputAsync("[green]Enter shift ID:[/]");
                else if (selected == "Cancel/Return to Menu") return -1;
                else
                {
                    var choices = BuildChoices(response, page, size);
                    var index = choices.IndexOf(selected);
                    if (index >= 0 && response.Data != null && index < response.Data.Count)
                    {
                        return response.Data[index].ShiftId;
                    }
                }
                return null;
            },
            1,
            10
        );
        return result as int? ?? -1;
    }

    private List<string> BuildChoices(ApiResponseDto<List<Shift>> response, int currentPage, int pageSize)
    {
        int startIndex = (currentPage - 1) * pageSize;
        var choices = response.Data?.Select((s, index) =>
            $"{startIndex + index + 1}. {s.StartTime:dd/MM/yyyy HH:mm} - {s.EndTime:dd/MM/yyyy HH:mm} ({s.Duration.TotalHours:F1}h)"
        ).ToList() ?? new List<string>();

        if (response.HasNextPage) choices.Add("Next Page...");
        if (response.HasPreviousPage) choices.Add("Previous Page...");
        choices.Add("Enter ID Manually");
        choices.Add("Cancel/Return to Menu");
        return choices;
    }

    // Implement abstract methods
    public override async Task<Shift> CreateUiAsync()
    {
        return await CreateShiftUi(0); // Adjust workerId as needed
    }

    public override async Task<Shift> UpdateUiAsync(Shift existingEntity)
    {
        return await UpdateShiftUi(existingEntity);
    }

    public override async Task<ShiftFilterOptions> FilterUiAsync()
    {
        return await FilterShiftsUi();
    }
}
