using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Spectre.Console;

namespace ConsoleFrontEnd.Controller;

public class LocationController
{
    private readonly MenuSystem.UserInterface userInterface = new();
    private readonly LocationService locationService = new();
    private LocationFilterOptions locationFilterOptions = new()
    {
        LocationId = null,
        Name = null,
        TownOrCity = null,
        StateOrCounty = null,
        ZipOrPostCode = null,
        Country = null,
        Search = null,
        SortBy = "Name", // Default sorting by name
        SortOrder = "asc", // Default sorting order is ascending
    };

    public async Task CreateLocation()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Create Location[/]").RuleStyle("yellow").Centered()
            );
            var location = userInterface.CreateLocationUi();
            var createdLocation = await locationService.CreateLocation(location);
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            userInterface.ContinueAndClearScreen();
        }
    }

    public async Task GetAllLocations()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View All Locations[/]").RuleStyle("yellow").Centered()
            );

            var filterOptions = userInterface.FilterLocationsUi();

            locationFilterOptions = filterOptions;
            var locations = await locationService.GetAllLocations(locationFilterOptions);

            if (locations.Data is not null)
            {
                userInterface.DisplayLocationsTable(locations.Data);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No locations found.[/]");
                userInterface.ContinueAndClearScreen();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    public async Task GetLocationById()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View Location by ID[/]").RuleStyle("yellow").Centered()
            );
            var locationId = userInterface.GetLocationByIdUi();
            var location = await locationService.GetLocationById(locationId);

            while (location.ResponseCode is System.Net.HttpStatusCode.NotFound)
            {
                var exitSelection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Try again or exit?")
                        .AddChoices(new[] { "Try Again", "Exit" })
                );
                if (exitSelection is "Exit")
                {
                    Console.Clear();
                    return;
                }
                else if (exitSelection is "Try Again")
                {
                    Console.Clear();
                    locationId = userInterface.GetLocationByIdUi();
                    location = await locationService.GetLocationById(locationId);
                }
            }

            userInterface.DisplayLocationsTable(location.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }

    public async Task UpdateLocation()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Update Location[/]").RuleStyle("yellow").Centered()
            );

            var locationId = userInterface.GetLocationByIdUi();

            var existingLocation = await locationService.GetLocationById(locationId);

            while (existingLocation.ResponseCode is System.Net.HttpStatusCode.NotFound)
            {
                var exitSelection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Try again or exit?")
                        .AddChoices(new[] { "Try Again", "Exit" })
                );
                if (exitSelection is "Exit")
                {
                    Console.Clear();
                    return;
                }
                else if (exitSelection is "Try Again")
                {
                    Console.Clear();
                    locationId = userInterface.GetLocationByIdUi();
                    existingLocation = await locationService.GetLocationById(locationId);
                }
            }

            // Safely convert List<Locations?> to List<Locations> and handle nulls
            var nonNullLocations = (existingLocation.Data ?? new List<Locations?>())
                .Where(w => w != null)
                .Cast<Locations>()
                .ToList();

            var updatedLocation = userInterface.UpdateLocationUi(nonNullLocations);

            var updatedLocationResponse = await locationService.UpdateLocation(
                locationId,
                updatedLocation
            );
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
        }
    }

    public async Task DeleteLocation()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Delete Location[/]").RuleStyle("yellow").Centered()
            );
            var locationId = userInterface.GetLocationByIdUi();
            var deletedLocation = await locationService.DeleteLocation(locationId);
            if (deletedLocation.ResponseCode is System.Net.HttpStatusCode.NotFound)
            {
                var exitSelection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Try again or exit?")
                        .AddChoices(new[] { "Try Again", "Exit" })
                );
                if (exitSelection == "Exit")
                {
                    Console.Clear();
                    return;
                }
                else
                {
                    Console.Clear();
                    await DeleteLocation(); // Retry
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try Pass failed in Location Controller: Delete Location {ex}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
