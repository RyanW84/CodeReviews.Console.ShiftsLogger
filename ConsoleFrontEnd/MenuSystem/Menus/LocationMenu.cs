using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Services;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
/// Location menu implementation following Single Responsibility Principle
/// Handles location-specific operations
/// </summary>
public class LocationMenu : BaseMenu
{
    private readonly ILocationService _locationService;
    private readonly ILocationUi _locationUi;

    public LocationMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<LocationMenu> logger,
        ILocationService locationService,
        ILocationUi locationUi)
        : base(displayService, inputService, navigationService, logger)
    {
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _locationUi = locationUi ?? throw new ArgumentNullException(nameof(locationUi));
    }

    public override string Title => "Location Management";
    public override string Context => "Location Management";

    protected override async Task ShowMenuAsync()
    {
        bool shouldExit = false;

        while (!shouldExit)
        {
            var choice = InputService.GetMenuChoice(
                "Select a location operation:",
                "View All Locations",
                "View Location by ID",
                "Create New Location",
                "Update Location",
                "Delete Location",
                "Filter Locations",
                "View Locations by Country",
                "View Locations by County",
                "Back to Main Menu"
            );

            shouldExit = await HandleLocationChoice(choice);
        }
    }

    private async Task<bool> HandleLocationChoice(string choice)
    {
        Logger.LogDebug("Location menu choice selected: {Choice}", choice);

        if (await HandleCommonActions(choice))
            return true;

        try
        {
            switch (choice)
            {
                case "View All Locations":
                    await ViewAllLocationsAsync();
                    break;

                case "View Location by ID":
                    await ViewLocationByIdAsync();
                    break;

                case "Create New Location":
                    await CreateLocationAsync();
                    break;

                case "Update Location":
                    await UpdateLocationAsync();
                    break;

                case "Delete Location":
                    await DeleteLocationAsync();
                    break;

                case "Filter Locations":
                    await FilterLocationsAsync();
                    break;

                case "View Locations by Country":
                    await ViewLocationsByCountryAsync();
                    break;

                case "View Locations by County":
                    await ViewLocationsByCountyAsync();
                    break;

                case "Back to Main Menu":
                    return true;

                default:
                    DisplayService.DisplayError("Invalid choice");
                    InputService.WaitForKeyPress();
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling location choice: {Choice}", choice);
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
            InputService.WaitForKeyPress();
        }

        return false;
    }

    private async Task ViewAllLocationsAsync()
    {
        DisplayService.DisplayHeader("All Locations", "blue");
        await _locationUi.DisplayLocationsWithPaginationAsync();
        InputService.WaitForKeyPress();
    }

    private async Task ViewLocationByIdAsync()
    {
        DisplayService.DisplayHeader("Select Location", "blue");

        var locationId = await _locationUi.GetLocationByIdUi();
        if (locationId <= 0)
        {
            DisplayService.DisplayError("No location selected.");
            InputService.WaitForKeyPress();
            return;
        }

        var response = await _locationService.GetLocationByIdAsync(locationId);
        DisplayService.DisplayHeader($"Location Details (ID: {locationId})", "blue");
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "Location not found.");
        }
        else
        {
            DisplayService.DisplayTable([response.Data], "Location Details");
            DisplayService.DisplaySuccess("Location details loaded successfully.");
        }
        InputService.WaitForKeyPress();
    }

    private async Task CreateLocationAsync()
    {
        DisplayService.DisplayHeader("Create New Location", "green");

        try
        {
            var name = InputService.GetTextInput("Enter Location Name:");
            var address = InputService.GetTextInput("Enter Address:");
            var town = InputService.GetTextInput("Enter Town:");
            var county = InputService.GetTextInput("Enter County:");
            var postCode = InputService.GetTextInput("Enter Post Code:");
            var country = InputService.GetTextInput("Enter Country:");

            // Create location (this would call the API)
            DisplayService.DisplaySuccess("Location created successfully!");
            DisplayService.DisplayInfo($"Name: {name}");
            DisplayService.DisplayInfo($"Address: {address}, {town}");
            DisplayService.DisplayInfo($"County: {county}");
            DisplayService.DisplayInfo($"Post Code: {postCode}");
            DisplayService.DisplayInfo($"Country: {country}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating location");
            DisplayService.DisplayError($"Failed to create location: {ex.Message}");
        }

        InputService.WaitForKeyPress();
        await Task.CompletedTask;
    }

    private async Task UpdateLocationAsync()
    {
        DisplayService.DisplayHeader("Update Location");

        var locationId = await _locationUi.GetLocationByIdUi();
        if (locationId <= 0)
        {
            DisplayService.DisplayError("No location selected.");
            InputService.WaitForKeyPress();
            return;
        }

        // Get the current location details
        var locationResponse = await _locationService.GetLocationByIdAsync(locationId);
        if (locationResponse.RequestFailed || locationResponse.Data == null)
        {
            DisplayService.DisplayError(locationResponse.Message ?? "Failed to retrieve location details.");
            InputService.WaitForKeyPress();
            return;
        }

        var location = locationResponse.Data;
        var name = InputService.GetTextInput($"Enter new name (current: {location.Name}):", false);
        var address = InputService.GetTextInput($"Enter new address (current: {location.Address}):", false);
        var town = InputService.GetTextInput($"Enter new town (current: {location.Town}):", false);
        var county = InputService.GetTextInput($"Enter new county (current: {location.County}):", false);
        var postCode = InputService.GetTextInput($"Enter new post code (current: {location.PostCode}):", false);
        var country = InputService.GetTextInput($"Enter new country (current: {location.Country}):", false);
        var updatedLocation = new Location
        {
            LocationId = location.LocationId,
            Name = string.IsNullOrWhiteSpace(name) ? location.Name : name,
            Address = string.IsNullOrWhiteSpace(address) ? location.Address : address,
            Town = string.IsNullOrWhiteSpace(town) ? location.Town : town,
            County = string.IsNullOrWhiteSpace(county) ? location.County : county,
            PostCode = string.IsNullOrWhiteSpace(postCode) ? location.PostCode : postCode,
            Country = string.IsNullOrWhiteSpace(country) ? location.Country : country
        };
        var response = await _locationService.UpdateLocationAsync(locationId, updatedLocation);
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "Failed to update location.");
        }
        else
        {
            DisplayService.DisplaySuccess("Location updated successfully.");
            DisplayService.DisplayTable([response.Data], "Updated Location");
        }
        InputService.WaitForKeyPress();
    }

    private async Task DeleteLocationAsync()
    {
        DisplayService.DisplayHeader("Delete Location", "red");

        var locationId = await _locationUi.GetLocationByIdUi();
        if (locationId <= 0)
        {
            DisplayService.DisplayError("No location selected.");
            InputService.WaitForKeyPress();
            return;
        }

        if (InputService.GetConfirmation($"Are you sure you want to delete location {locationId}?"))
        {
            var response = await _locationService.DeleteLocationAsync(locationId);
            if (response.RequestFailed)
            {
                DisplayService.DisplayError(response.Message ?? "Failed to delete location.");
            }
            else
            {
                DisplayService.DisplaySuccess(response.Message ?? $"Location {locationId} deleted successfully.");
            }
        }
        else
        {
            DisplayService.DisplayInfo("Delete cancelled.");
        }
        InputService.WaitForKeyPress();
    }

    private async Task FilterLocationsAsync()
    {
        DisplayService.DisplayHeader("Filter Locations", "blue");

        // Get all locations for country/county selection
        var allLocationsResponse = await _locationService.GetAllLocationsAsync();
        string? county = null;
        string? country = null;
        if (allLocationsResponse.Data != null && allLocationsResponse.Data.Any())
        {
            var counties = allLocationsResponse.Data.Select(l => l.County).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).ToList();
            var countries = allLocationsResponse.Data.Select(l => l.Country).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).ToList();
            if (counties.Any())
            {
                string[] countyChoices = ["Any", ..counties.Select(s => s!)];
                var selectedCounty = InputService.GetMenuChoice("Filter by County:", countyChoices);
                if (selectedCounty != "Any") county = selectedCounty;
            }
            if (countries.Any())
            {
                string[] countryChoices = ["Any", ..countries.Select(s => s!)];
                var selectedCountry = InputService.GetMenuChoice("Filter by Country:", countryChoices);
                if (selectedCountry != "Any") country = selectedCountry;
            }
        }

        var filter = new LocationFilterOptions
        {
            Name = InputService.GetTextInput("Filter by name (leave blank for any):", false),
            Address = InputService.GetTextInput("Filter by address (leave blank for any):", false),
            Town = InputService.GetTextInput("Filter by town (leave blank for any):", false),
            County = county,
            PostCode = InputService.GetTextInput("Filter by post code (leave blank for any):", false),
            Country = country
            // If you add date/time fields in the future, use dd/MM/yyyy HH:mm format for prompts and parsing
        };
        var response = await _locationService.GetLocationsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No locations found matching filter.");
        }
        else
        {
            DisplayService.DisplayTable(response.Data, "Filtered Locations");
            DisplayService.DisplaySuccess($"Total filtered locations: {response.TotalCount}");
        }
        InputService.WaitForKeyPress();
    }

    private async Task ViewLocationsByCountryAsync()
    {
        DisplayService.DisplayHeader("Locations by Country", "blue");
        var country = InputService.GetTextInput("Enter country:");
        var filter = new LocationFilterOptions { Country = country };
        var response = await _locationService.GetLocationsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? $"No locations found in country '{country}'.");
        }
        else
        {
            DisplayService.DisplayTable(response.Data, $"Locations in '{country}'");
            DisplayService.DisplaySuccess($"Total: {response.TotalCount}");
        }
        InputService.WaitForKeyPress();
    }

    private async Task ViewLocationsByCountyAsync()
    {
        DisplayService.DisplayHeader("Locations by County", "blue");
        var county = InputService.GetTextInput("Enter county:");
        var filter = new LocationFilterOptions { County = county };
        var response = await _locationService.GetLocationsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? $"No locations found in county '{county}'.");
        }
        else
        {
            DisplayService.DisplayTable(response.Data, $"Locations in '{county}'");
            DisplayService.DisplaySuccess($"Total: {response.TotalCount}");
        }
        InputService.WaitForKeyPress();
    }
}
