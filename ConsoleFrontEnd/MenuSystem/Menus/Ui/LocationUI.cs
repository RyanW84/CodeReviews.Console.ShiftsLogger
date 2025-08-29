using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class LocationUI : ILocationUi
{
    private readonly IConsoleDisplayService _display;
    private readonly UiHelper _uiHelper;
    private readonly ILocationService _locationService;

    public LocationUI(IConsoleDisplayService display, ILogger<LocationUI> logger, ILocationService locationService)
    {
        _display = display;
        _uiHelper = new UiHelper(display, logger);
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
    }

    public Location CreateLocationUi()
    {
        _display.DisplayHeader("Create New Location");
        
        var name = AnsiConsole.Ask<string>("[green]Enter location name:[/]");
        var address = AnsiConsole.Ask<string>("[green]Enter address:[/]");
        var town = AnsiConsole.Ask<string>("[green]Enter town:[/]");
        var county = AnsiConsole.Ask<string>("[green]Enter county:[/]");
        var postCode = AnsiConsole.Ask<string>("[green]Enter post code:[/]");
        var country = AnsiConsole.Ask<string>("[green]Enter country:[/]");
        
        return new Location
        {
            LocationId = 0, // Will be assigned by service
            Name = name,
            Address = address,
            Town = town,
            County = county,
            PostCode = postCode,
            Country = country
        };
    }

    public Location UpdateLocationUi(Location existingLocation)
    {
        _display.DisplayHeader($"Update Location: {existingLocation.Name}");
        
        var name = AnsiConsole.Ask("[green]Enter new location name:[/]", existingLocation.Name);
        var address = AnsiConsole.Ask("[green]Enter address:[/]", existingLocation.Address);
        var town = AnsiConsole.Ask("[green]Enter town:[/]", existingLocation.Town);
        var county = AnsiConsole.Ask("[green]Enter county:[/]", existingLocation.County);
        var postCode = AnsiConsole.Ask("[green]Enter post code:[/]", existingLocation.PostCode);
        var country = AnsiConsole.Ask("[green]Enter country:[/]", existingLocation.Country);
        
        return new Location
        {
            LocationId = existingLocation.Id,
            Name = name,
            Address = address,
            Town = town,
            County = county,
            PostCode = postCode,
            Country = country
        };
    }

    public LocationFilterOptions FilterLocationsUi()
    {
        _display.DisplayHeader("Filter Locations");
        
        var name = _uiHelper.GetOptionalStringInput("Filter by name");
        
        return new LocationFilterOptions
        {
            Name = name
        };
    }

    public void DisplayLocationsTable(IEnumerable<Location> locations, int startingRowNumber = 1)
    {
        _display.DisplayTable(locations, "Locations", startingRowNumber);
    }

    public async Task DisplayLocationsWithPaginationAsync(int initialPageNumber = 1, int pageSize = 10)
    {
        var currentPage = initialPageNumber;

        while (true)
        {
            _display.DisplayHeader($"Locations (Page {currentPage})", "blue");

            var response = await _locationService.GetAllLocationsAsync(currentPage, pageSize).ConfigureAwait(false);

            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    _display.DisplayError("No locations found.");
                    return;
                }
                else
                {
                    _display.DisplayError($"No locations found on page {currentPage}. Returning to page 1.");
                    currentPage = 1;
                    continue;
                }
            }

            // Calculate starting index for continuous numbering across pages
            int startIndex = (currentPage - 1) * pageSize;

            DisplayLocationsTable(response.Data, startIndex + 1);

            // Display pagination info
            _display.DisplayInfo($"Page {response.PageNumber} of {response.TotalPages} | Total: {response.TotalCount} locations");
            _display.DisplayInfo($"Showing {response.Data.Count()} of {response.TotalCount} locations");

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

    public async Task<int> GetLocationByIdUi()
    {
        _display.DisplayHeader("Select Location", "blue");

        var currentPage = 1;
        const int pageSize = 10;

        while (true)
        {
            var response = await _locationService.GetAllLocationsAsync(currentPage, pageSize).ConfigureAwait(false);
            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    _uiHelper.DisplayValidationError(response.Message ?? "No locations available.");
                    // Fallback to manual entry
                    return AnsiConsole.Ask<int>("[green]Enter location ID:[/]");
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
                .Select((l, index) => $"{startIndex + index + 1}. {l.Name} - {l.Town}, {l.Country}")
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
                    .Title($"Select Location (Page {response.PageNumber} of {response.TotalPages}):")
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
                return AnsiConsole.Ask<int>("[green]Enter location ID:[/]");
            }
            else if (selected == "Cancel/Return to Menu")
            {
                return -1;
            }
            else
            {
                // Extract the count from the selected choice and get the corresponding location
                var count = UiHelper.ExtractCountFromChoice(selected);
                if (count > 0 && count <= response.Data.Count)
                {
                    return response.Data[count - 1].LocationId;
                }
                else
                {
                    _display.DisplayError("Invalid selection.");
                    continue;
                }
            }
        }
    }

    public async Task<int> SelectLocation()
    {
        _display.DisplayHeader("Select Location", "blue");

        var currentPage = 1;
        const int pageSize = 10;

        while (true)
        {
            var response = await _locationService.GetAllLocationsAsync(currentPage, pageSize).ConfigureAwait(false);
            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    _uiHelper.DisplayValidationError(response.Message ?? "No locations available.");
                    // Fallback to manual entry
                    return AnsiConsole.Ask<int>("[green]Select location ID:[/]");
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
                .Select((l, index) => $"{startIndex + index + 1}. {l.Name} - {l.Town}, {l.Country}")
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
                    .Title($"Select Location (Page {response.PageNumber} of {response.TotalPages}):")
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
                return AnsiConsole.Ask<int>("[green]Enter location ID:[/]");
            }
            else if (selected == "Cancel/Return to Menu")
            {
                return -1;
            }
            else
            {
                // Extract the count from the selected choice and get the corresponding location
                var count = UiHelper.ExtractCountFromChoice(selected);
                if (count > 0 && count <= response.Data.Count)
                {
                    return response.Data[count - 1].LocationId;
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
