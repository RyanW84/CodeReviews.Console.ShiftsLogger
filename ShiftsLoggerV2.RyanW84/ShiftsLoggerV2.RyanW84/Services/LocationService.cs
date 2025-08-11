using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Services;

public class LocationService(ShiftsLoggerDbContext dbContext) : ILocationService
{
    public async Task<ApiResponseDto<List<Locations>>> GetAllLocations(
        LocationFilterOptions locationOptions
    )
    {
        AnsiConsole.MarkupLine(
            $"[yellow]Filter options received:[/]\n"
                + $"  [blue]LocationId:[/] {locationOptions.LocationId ?? null}\n"
                + $"  [blue]Name:[/] {locationOptions.Name ?? null}\n"
                + $"  [blue]SortBy:[/] {locationOptions.SortBy ?? "null"}\n"
                + $"  [blue]SortOrder:[/] {locationOptions.SortOrder ?? "null"}\n"
                + $"  [blue]Search:[/] '{locationOptions.Search ?? "null"}'"
        );

        var query = dbContext.Locations.AsQueryable();

		ApplyFilters(ref query, locationOptions);

        // Sorting
        if (!string.IsNullOrWhiteSpace(locationOptions.SortBy))
        {
            locationOptions.SortBy = locationOptions.SortBy.ToLowerInvariant();
            locationOptions.SortOrder = locationOptions.SortOrder?.ToLowerInvariant();
        }
        else
        {
            locationOptions.SortBy = "locationid";
        }

        AnsiConsole.MarkupLine(
            $"[yellow]Applying sorting:[/] SortBy='{locationOptions.SortBy}', SortOrder='{locationOptions.SortOrder}'"
        );

        query = locationOptions.SortBy switch
        {
            "locationid" => locationOptions.SortOrder == "desc"
                ? query.OrderByDescending(l => l.LocationId)
                : query.OrderBy(l => l.LocationId),
            "name" => locationOptions.SortOrder == "desc"
                ? query.OrderByDescending(l => l.Name)
                : query.OrderBy(l => l.Name),
            "address" => locationOptions.SortOrder == "desc"
                ? query.OrderByDescending(l => l.Address)
                : query.OrderBy(l => l.Address),
            "townorcity" => locationOptions.SortOrder == "desc"
                ? query.OrderByDescending(l => l.TownOrCity)
                : query.OrderBy(l => l.TownOrCity),
            "stateorcounty" => locationOptions.SortOrder == "desc"
                ? query.OrderByDescending(l => l.StateOrCounty)
                : query.OrderBy(l => l.StateOrCounty),
            "ziporpostcode" => locationOptions.SortOrder == "desc"
                ? query.OrderByDescending(l => l.ZipOrPostCode)
                : query.OrderBy(l => l.ZipOrPostCode),
            "country" => locationOptions.SortOrder == "desc"
                ? query.OrderByDescending(l => l.Country)
                : query.OrderBy(l => l.Country),
            _ => query.OrderBy(l => l.LocationId)
        };

        AnsiConsole.MarkupLine("[yellow]Executing final query...[/]");

        var locations = await query.ToListAsync();

        if (locations.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No locations found with the specified criteria.[/]");
            return new ApiResponseDto<List<Locations>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.NotFound,
                Message = "No locations found with the specified criteria.",
                Data = locations,
                TotalCount = 0
            };
        }

        AnsiConsole.MarkupLine(
            $"[green]Successfully retrieved {locations.Count} locations, sorted by '{locationOptions.SortBy}' in {locationOptions.SortOrder} order.[/]"
        );
        return new ApiResponseDto<List<Locations>>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = "Locations retrieved successfully.",
            Data = locations,
            TotalCount = locations.Count
        };
    }

    public async Task<ApiResponseDto<Locations?>> GetLocationById(int id)
    {
        Locations? location = await dbContext.Locations.FirstOrDefaultAsync<Locations> (l=> l.LocationId == id);

		if (location is null)
        {
            return new ApiResponseDto<Locations?>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Location with ID: {id} not found.",
                Data = null,
                TotalCount = 0
            };
        }

        AnsiConsole.MarkupLine(
            $"[green]Successfully retrieved location with ID: {location.LocationId}.[/]"
        );
        return new ApiResponseDto<Locations?>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = $"Location with ID: {id} retrieved successfully.",
            Data = location,
            TotalCount = 1
        };
    }

    public async Task<ApiResponseDto<Locations>> CreateLocation(LocationApiRequestDto location)
    {
        // Basic parameter validation
        if (string.IsNullOrWhiteSpace(location.Name) ||
            string.IsNullOrWhiteSpace(location.Address) ||
            string.IsNullOrWhiteSpace(location.TownOrCity) ||
            string.IsNullOrWhiteSpace(location.StateOrCounty) ||
            string.IsNullOrWhiteSpace(location.ZipOrPostCode) ||
            string.IsNullOrWhiteSpace(location.Country))
        {
            return new ApiResponseDto<Locations>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.BadRequest,
                Message = "All fields are required.",
                Data = null,
                TotalCount = 0
            };
        }

        try
        {
            Locations newLocation = new()
            {
                Name = location.Name,
                Address = location.Address,
                TownOrCity = location.TownOrCity,
                StateOrCounty = location.StateOrCounty,
                ZipOrPostCode = location.ZipOrPostCode,
                Country = location.Country,
            };
            var savedLocation = await dbContext.Locations.AddAsync(newLocation);
            await dbContext.SaveChangesAsync();

            AnsiConsole.MarkupLine(
                $"\n[green]Successfully created location with ID: {savedLocation.Entity.LocationId}[/]"
            );

            return new ApiResponseDto<Locations>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.Created,
                Message = "Location created successfully.",
                Data = savedLocation.Entity,
                TotalCount = 1
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocationService][CreateLocation] Exception: {ex}");
            return new ApiResponseDto<Locations>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "An error occurred while creating the location.",
                Data = null,
                TotalCount = 0
            };
        }
    }

    public async Task<ApiResponseDto<Locations?>> UpdateLocation(
        int id,
        LocationApiRequestDto updatedLocation
    )
    {
        if (string.IsNullOrWhiteSpace(updatedLocation.Name) ||
            string.IsNullOrWhiteSpace(updatedLocation.Address) ||
            string.IsNullOrWhiteSpace(updatedLocation.TownOrCity) ||
            string.IsNullOrWhiteSpace(updatedLocation.StateOrCounty) ||
            string.IsNullOrWhiteSpace(updatedLocation.ZipOrPostCode) ||
            string.IsNullOrWhiteSpace(updatedLocation.Country))
        {
            return new ApiResponseDto<Locations?>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.BadRequest,
                Message = "All fields are required.",
                Data = null,
                TotalCount = 0
            };
        }

        Locations? savedLocation = await dbContext.Locations.FindAsync(id);

        if (savedLocation is null)
        {
            return new ApiResponseDto<Locations?>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Location with ID: {id} not found.",
                Data = null,
                TotalCount = 0
            };
        }

        savedLocation.Name = updatedLocation.Name;
        savedLocation.Address = updatedLocation.Address;
        savedLocation.TownOrCity = updatedLocation.TownOrCity;
        savedLocation.StateOrCounty = updatedLocation.StateOrCounty;
        savedLocation.ZipOrPostCode = updatedLocation.ZipOrPostCode;
        savedLocation.Country = updatedLocation.Country;

        dbContext.Locations.Update(savedLocation);
        await dbContext.SaveChangesAsync();

        return new ApiResponseDto<Locations?>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = $"Location with ID: {id} updated successfully.",
            Data = savedLocation,
            TotalCount = 1
        };
    }

    public async Task<ApiResponseDto<string?>> DeleteLocation(int id)
    {
        Locations? savedLocation = await dbContext.Locations.FindAsync(id);

        if (savedLocation is null)
        {
            return new ApiResponseDto<string?>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Location with ID: {id} not found.",
                Data = null,
                TotalCount = 0
            };
        }

        dbContext.Locations.Remove(savedLocation);
        await dbContext.SaveChangesAsync();

        return new ApiResponseDto<string?>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = $"Location with ID: {id} deleted successfully.",
            Data = null,
            TotalCount = 1
        };
    }

    // Private helper for filtering
    private static void ApplyFilters(ref IQueryable<Locations> query, LocationFilterOptions options)
    {
        if (options.LocationId != null && options.LocationId is not 0)
        {
            query = query.Where(l => l.LocationId == options.LocationId);
        }
        if (!string.IsNullOrWhiteSpace(options.Name))
        {
            query = query.Where(l => EF.Functions.Like(l.Name, $"%{options.Name}%"));
        }
        if (!string.IsNullOrWhiteSpace(options.Address))
        {
            query = query.Where(l => EF.Functions.Like(l.Address, $"%{options.Address}%"));
        }
        if (!string.IsNullOrWhiteSpace(options.TownOrCity))
        {
            query = query.Where(l => EF.Functions.Like(l.TownOrCity, $"%{options.TownOrCity}%"));
        }
        if (!string.IsNullOrWhiteSpace(options.StateOrCounty))
        {
            query = query.Where(l => EF.Functions.Like(l.StateOrCounty, $"%{options.StateOrCounty}%"));
        }
        if (!string.IsNullOrWhiteSpace(options.ZipOrPostCode))
        {
            query = query.Where(l => EF.Functions.Like(l.ZipOrPostCode, $"%{options.ZipOrPostCode}%"));
        }
        if (!string.IsNullOrWhiteSpace(options.Country))
        {
            query = query.Where(l => EF.Functions.Like(l.Country, $"%{options.Country}%"));
        }
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(l =>
                l.LocationId.ToString().Contains(options.Search)
                || EF.Functions.Like(l.Name, $"%{options.Search}%")
                || EF.Functions.Like(l.Address, $"%{options.Search}%")
                || EF.Functions.Like(l.TownOrCity, $"%{options.Search}%")
                || EF.Functions.Like(l.StateOrCounty, $"%{options.Search}%")
                || EF.Functions.Like(l.ZipOrPostCode, $"%{options.Search}%")
                || EF.Functions.Like(l.Country, $"%{options.Search}%")
            );
        }
    }
}
