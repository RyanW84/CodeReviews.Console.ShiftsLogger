using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem.Base;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class LocationUI : BaseEntityUi<Location, LocationFilterOptions>, ILocationUi
{
    private readonly UiHelper _uiHelper;
    private readonly ILocationService _locationService;
    private readonly PaginationHandler _paginationHandler;

    public LocationUI(
        IConsoleDisplayService display,
        IConsoleInputService input,
        ILogger<LocationUI> logger,
        ILocationService locationService
    ) : base(display, logger)
    {
        _uiHelper = new UiHelper(display, logger);
        _locationService =
            locationService ?? throw new ArgumentNullException(nameof(locationService));
        _paginationHandler = new PaginationHandler(display, input);
    }

    protected override string EntityName => "Location";
    protected override string EntityPluralName => "Locations";

    public override async Task<Location> CreateUiAsync()
    {
        await Task.CompletedTask; // For async compatibility
        _display.DisplayHeader("Create New Location");

        var name = AnsiConsole.Ask<string>("[green]Enter location name:[/]");
        var address = AnsiConsole.Ask<string>("[green]Enter address:[/]");
        var town = AnsiConsole.Ask<string>("[green]Enter town:[/]");
        var county = AnsiConsole.Ask<string>("[green]Enter county:[/]");
        var postcode = AnsiConsole.Ask<string>("[green]Enter postcode:[/]");
        var country = AnsiConsole.Ask<string>("[green]Enter country:[/]");

        return new Location
        {
            LocationId = 0, // Will be assigned by service
            Name = name,
            Address = address,
            Town = town,
            County = county,
            Country = country,
            Postcode = postcode,
        };
    }

    public async Task<Location> CreateLocationUiAsync()
    {
        return await CreateUiAsync();
    }

    public Location CreateLocationUi()
    {
        return CreateUiAsync().GetAwaiter().GetResult();
    }

    public override async Task<Location> UpdateUiAsync(Location existingLocation)
    {
        await Task.CompletedTask;
        _display.DisplayHeader($"Update Location: {existingLocation.Name}");

        var name = AnsiConsole.Ask("[green]Enter new location name:[/]", existingLocation.Name);
        var address = AnsiConsole.Ask("[green]Enter address:[/]", existingLocation.Address);
        var town = AnsiConsole.Ask("[green]Enter town:[/]", existingLocation.Town);
        var county = AnsiConsole.Ask("[green]Enter county:[/]", existingLocation.County);
        var postcode = AnsiConsole.Ask("[green]Enter post code:[/]", existingLocation.Postcode);
        var country = AnsiConsole.Ask("[green]Enter country:[/]", existingLocation.Country);

        return new Location
        {
            LocationId = existingLocation.Id,
            Name = name,
            Address = address,
            Town = town,
            County = county,
            Postcode = postcode,
            Country = country,
        };
    }

    public async Task<Location> UpdateLocationUiAsync(Location existingLocation)
    {
        return await UpdateUiAsync(existingLocation);
    }

    public Location UpdateLocationUi(Location existingLocation)
    {
        return UpdateUiAsync(existingLocation).GetAwaiter().GetResult();
    }

    public override async Task<LocationFilterOptions> FilterUiAsync()
    {
        await Task.CompletedTask;
        _display.DisplayHeader("Filter Locations");

        var name = _uiHelper.GetOptionalStringInput("Filter by name");

        return new LocationFilterOptions { Name = name };
    }

    public async Task<LocationFilterOptions> FilterLocationsUiAsync()
    {
        return await FilterUiAsync();
    }

    public LocationFilterOptions FilterLocationsUi()
    {
        return FilterUiAsync().GetAwaiter().GetResult();
    }

    public override async Task<int> GetEntityByIdUiAsync()
    {
        return await GetLocationByIdUi();
    }

    public override void DisplayEntitiesTable(IEnumerable<Location> entities)
    {
        DisplayLocationsTable(entities, 1);
    }

    public void DisplayLocationsTable(IEnumerable<Location> locations, int startingRowNumber = 1)
    {
        _display.DisplayTable(locations, "Locations", startingRowNumber);
    }

    public async Task<(bool Selected, int LocationId)> DisplayLocationsWithPaginationAsync(
        int initialPageNumber = 1,
        int pageSize = 10
    )
    {
        var currentPage = initialPageNumber;

        while (true)
        {
            _display.DisplayHeader($"Locations (Page {currentPage})", "blue");

            var response = await _locationService
                .GetAllLocationsAsync(currentPage, pageSize)
                .ConfigureAwait(false);

            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    _display.DisplayError("No locations found.");
                    return (false, -1);
                }
                else
                {
                    _display.DisplayError(
                        $"No locations found on page {currentPage}. Returning to page 1."
                    );
                    currentPage = 1;
                    continue;
                }
            }

            // Calculate starting index for continuous numbering across pages
            int startIndex = (currentPage - 1) * pageSize;

            DisplayLocationsTable(response.Data, startIndex + 1);

            // Display pagination info
            _display.DisplayInfo(
                $"Page {response.PageNumber} of {response.TotalPages} | Total: {response.TotalCount} locations"
            );
            _display.DisplayInfo(
                $"Showing {response.Data.Count()} of {response.TotalCount} locations"
            );

            // Create pagination options
            var options = new List<string>();

            if (response.HasPreviousPage)
                options.Add("Previous Page");

            if (response.HasNextPage)
                options.Add("Next Page");

            options.Add("Go to Page");
            options.Add("Change Page Size");
            options.Add("Select Location");
            options.Add("Back to Menu");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Choose an action:").AddChoices(options)
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
                    var pageInput = AnsiConsole.Ask<int>(
                        $"Enter page number (1-{response.TotalPages}):"
                    );
                    if (pageInput >= 1 && pageInput <= response.TotalPages)
                        currentPage = pageInput;
                    else
                        _display.DisplayError(
                            $"Invalid page number. Please enter a number between 1 and {response.TotalPages}."
                        );
                    break;

                case "Change Page Size":
                    var sizeInput = AnsiConsole.Ask<int>("Enter new page size (1-100):");
                    if (sizeInput >= 1 && sizeInput <= 100)
                    {
                        pageSize = sizeInput;
                        currentPage = 1; // Reset to first page
                    }
                    else
                        _display.DisplayError(
                            "Invalid page size. Please enter a number between 1 and 100."
                        );
                    break;

                case "Select Location":
                    // Return the current page data for selection
                    var selectedLocationId = await GetLocationByIdUi();
                    return (true, selectedLocationId);

                case "Back to Menu":
                    return (false, -1);
            }
        }
    }

    public async Task<int> GetLocationByIdUi()
    {
        var result = await _paginationHandler.HandlePaginationAsync(
            async (page, size) => await _locationService.GetAllLocationsAsync(page, size),
            (response, page, size) => DisplayPage(response, page, size),
            BuildChoices,
            async (selected, response, page, size) =>
            {
                await Task.CompletedTask;
                if (selected == "Next Page...") return null; // Continue pagination
                else if (selected == "Previous Page...") return null;
                else if (selected == "Enter ID Manually") return AnsiConsole.Ask<int>("[green]Enter location ID:[/]");
                else if (selected == "Cancel/Return to Menu") return -1;
                else
                {
                    var choices = BuildChoices(response, page, size);
                    var index = choices.IndexOf(selected);
                    if (index >= 0 && response.Data != null && index < response.Data.Count)
                    {
                        return response.Data[index].LocationId;
                    }
                }
                return null;
            },
            1,
            10
        );
        return result as int? ?? -1;
    }

    private void DisplayPage(ApiResponseDto<List<Location>> response, int currentPage, int pageSize)
    {
        int startIndex = (currentPage - 1) * pageSize;
        if (response.Data != null) DisplayLocationsTable(response.Data, startIndex + 1);
        _display.DisplayInfo($"Page {response.PageNumber} of {response.TotalPages} | Total: {response.TotalCount} locations");
    }

    private List<string> BuildChoices(ApiResponseDto<List<Location>> response, int currentPage, int pageSize)
    {
        int startIndex = (currentPage - 1) * pageSize;
        var choices = response.Data?.Select((l, index) =>
            $"{startIndex + index + 1}. {l.Name} - {l.Town}, {l.Country}"
        ).ToList() ?? new List<string>();

        if (response.HasNextPage) choices.Add("Next Page...");
        if (response.HasPreviousPage) choices.Add("Previous Page...");
        choices.Add("Enter ID Manually");
        choices.Add("Cancel/Return to Menu");
        return choices;
    }

    public async Task<int> SelectLocation()
    {
        return await GetLocationByIdUi();
    }
}
