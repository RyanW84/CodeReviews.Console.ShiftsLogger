using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
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
    private readonly Dictionary<string, Func<Task<bool>>> MenuActions;

    public LocationMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<LocationMenu> logger,
        ILocationService locationService,
        ILocationUi locationUi
    )
        : base(displayService, inputService, navigationService, logger)
    {
        _locationService =
            locationService ?? throw new ArgumentNullException(nameof(locationService));
        _locationUi = locationUi ?? throw new ArgumentNullException(nameof(locationUi));

        MenuActions = new Dictionary<string, Func<Task<bool>>>
        {
            ["View All Locations"] = async () => { await ViewAllLocationsAsync(); return false; },
            ["View Location by ID"] = async () => { await ViewLocationByIdAsync(); return false; },
            ["Create New Location"] = async () => { await CreateLocationAsync(); return false; },
            ["Update Location"] = async () => { await UpdateLocationAsync(); return false; },
            ["Delete Location"] = async () => { await DeleteLocationAsync(); return false; },
            ["Filter Locations"] = async () => { await FilterLocationsAsync(); return false; },
            ["View Locations by Country"] = async () => { await ViewLocationsByCountryAsync(); return false; },
            ["View Locations by County"] = async () => { await ViewLocationsByCountyAsync(); return false; },
            ["Back to Main Menu"] = () => Task.FromResult(true)
        };
    }

    public override string Title => "Location Management";
    public override string Context => "Location Management";

    protected override async Task ShowMenuAsync()
    {
        bool shouldExit = false;

        while (!shouldExit)
        {
            var choice = await InputService.GetMenuChoiceAsync(
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

    private async Task ViewAllLocationsAsync()
    {
        DisplayService.DisplayHeader("All Locations", "blue");
        var (selected, locationId) = await _locationUi.DisplayLocationsWithPaginationAsync();

        if (selected && locationId > 0)
        {
            // User selected a location, display its details
            var response = await _locationService.GetLocationByIdAsync(locationId);
            if (response.RequestFailed)
            {
                DisplayService.DisplayError($"Failed to retrieve location: {response.Message}");
            }
            else if (response.Data == null)
            {
                DisplayService.DisplayError("Location not found.");
            }
            else
            {
                await DisplayLocationDetails(response.Data);
            }
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task ViewLocationByIdAsync()
    {
        try
        {
            DisplayService.DisplayHeader("View Location by ID", "blue");

            var locationId = await _locationUi.GetLocationByIdUi();
            if (locationId <= 0)
            {
                DisplayService.DisplayInfo("Operation cancelled.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            var response = await _locationService.GetLocationByIdAsync(locationId);

            if (response.RequestFailed)
            {
                DisplayService.DisplayError($"Failed to retrieve location: {response.Message}");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            if (response.Data == null)
            {
                DisplayService.DisplayError("Location not found.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            await DisplayLocationDetails(response.Data);
        }
        catch (Exception ex)
        {
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
            await InputService.WaitForKeyPressAsync();
        }
    }

    private async Task CreateLocationAsync()
    {
        DisplayService.DisplayHeader("Create New Location", "green");

        try
        {
            var location = await _locationUi.CreateUiAsync();
            var response = await _locationService.CreateLocationAsync(location);
            if (response.RequestFailed || response.Data == null)
            {
                DisplayService.DisplayError(response.Message ?? "Failed to create location.");
            }
            else
            {
                DisplayService.DisplaySuccess("Location created successfully!");
                DisplayService.DisplayTable([response.Data], "Created Location");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating location");
            DisplayService.DisplayError($"Failed to create location: {ex.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task UpdateLocationAsync()
    {
        DisplayService.DisplayHeader("Update Location");

        var locationId = await _locationUi.GetLocationByIdUi();
        if (locationId <= 0)
        {
            DisplayService.DisplayError("No location selected.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Get the current location details
        var locationResponse = await _locationService.GetLocationByIdAsync(locationId);
        if (locationResponse.RequestFailed || locationResponse.Data == null)
        {
            DisplayService.DisplayError(
                locationResponse.Message ?? "Failed to retrieve location details."
            );
            await InputService.WaitForKeyPressAsync();
            return;
        }

        var location = locationResponse.Data;
        var updatedLocation = await _locationUi.UpdateUiAsync(location);
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
        await InputService.WaitForKeyPressAsync();
    }

    private async Task DeleteLocationAsync()
    {
        DisplayService.DisplayHeader("Delete Location", "red");

        var locationId = await _locationUi.GetLocationByIdUi();
        if (locationId <= 0)
        {
            DisplayService.DisplayError("No location selected.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Get location details for confirmation
        var response = await _locationService.GetLocationByIdAsync(locationId);
        if (response.RequestFailed)
        {
            DisplayService.DisplayError($"Failed to retrieve location: {response.Message}");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        if (response.Data == null)
        {
            DisplayService.DisplayError("Location not found.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Display location details before confirmation
        DisplayService.DisplayHeader("Location to Delete", "yellow");
        DisplayService.DisplayTable([response.Data], "Location Details");

        if (await InputService.GetConfirmationAsync($"Are you sure you want to delete this location?"))
        {
            var deleteResponse = await _locationService.DeleteLocationAsync(locationId);
            if (deleteResponse.RequestFailed)
            {
                DisplayService.DisplayError(deleteResponse.Message ?? "Failed to delete location.");
            }
            else
            {
                DisplayService.DisplaySuccess(
                    deleteResponse.Message ?? $"Location deleted successfully."
                );
            }
        }
        else
        {
            DisplayService.DisplayInfo("Delete cancelled.");
        }
        await InputService.WaitForKeyPressAsync();
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
            var counties = allLocationsResponse
                .Data.Select(l => l.County)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            var countries = allLocationsResponse
                .Data.Select(l => l.Country)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            if (counties.Any())
            {
                string[] countyChoices = ["Any", .. counties.Select(s => s!)];
                var selectedCounty = await InputService.GetMenuChoiceAsync("Filter by County:", countyChoices);
                if (selectedCounty != "Any")
                    county = selectedCounty;
            }
            if (countries.Any())
            {
                string[] countryChoices = ["Any", .. countries.Select(s => s!)];
                var selectedCountry = await InputService.GetMenuChoiceAsync(
                    "Filter by Country:",
                    countryChoices
                );
                if (selectedCountry != "Any")
                    country = selectedCountry;
            }
        }

        var filter = new LocationFilterOptions
        {
            Name = await InputService.GetTextInputAsync("Filter by name (leave blank for any):", false),
            Address = await InputService.GetTextInputAsync("Filter by address (leave blank for any):", false),
            Town = await InputService.GetTextInputAsync("Filter by town (leave blank for any):", false),
            County = county,
            PostCode = await InputService.GetTextInputAsync(
                "Filter by post code (leave blank for any):",
                false
            ),
            Country = country,
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
        await InputService.WaitForKeyPressAsync();
    }

    private async Task ViewLocationsByCountryAsync()
    {
        DisplayService.DisplayHeader("Locations by Country", "blue");
        var country = await InputService.GetTextInputAsync("Enter country:");
        var filter = new LocationFilterOptions { Country = country };
        var response = await _locationService.GetLocationsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(
                response.Message ?? $"No locations found in country '{country}'."
            );
        }
        else
        {
            DisplayService.DisplayTable(response.Data, $"Locations in '{country}'");
            DisplayService.DisplaySuccess($"Total: {response.TotalCount}");
        }
        await InputService.WaitForKeyPressAsync();
    }

    private async Task ViewLocationsByCountyAsync()
    {
        DisplayService.DisplayHeader("Locations by County", "blue");
        var county = await InputService.GetTextInputAsync("Enter county:");
        var filter = new LocationFilterOptions { County = county };
        var response = await _locationService.GetLocationsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(
                response.Message ?? $"No locations found in county '{county}'."
            );
        }
        else
        {
            DisplayService.DisplayTable(response.Data, $"Locations in '{county}'");
            DisplayService.DisplaySuccess($"Total: {response.TotalCount}");
        }
        await InputService.WaitForKeyPressAsync();
    }

    private async Task DisplayLocationDetails(Location location)
    {
        DisplayService.DisplayHeader("Location Details", "green");
        DisplayService.DisplayTable([location], "Location Details");
        await InputService.WaitForKeyPressAsync();
    }
}
