using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Services;

public class LocationService(ShiftsLoggerDbContext dbContext) : ILocationService
{
    public async Task<ApiResponseDto<List<Locations?>>> GetAllLocations(
        LocationFilterOptions locationOptions
    )
    {
        AnsiConsole.MarkupLine(
            $"[yellow]Filter options received:[/]\n"
                + $"  [blue]LocationId:[/] {locationOptions.LocationId ?? null}\n"
                + $"  [blue]LocationId:[/] {locationOptions.Name ?? null}\n"
                + $"  [blue]SortBy:[/] {locationOptions.SortBy ?? "null"}\n"
                + $"  [blue]SortOrder:[/] {locationOptions.SortOrder ?? "null"}\n"
                + $"  [blue]Search:[/] '{locationOptions.Search ?? "null"}'"
        );

        var query = dbContext
            .Locations.Include(l => l.Workers)
            .Include(l => l.Shifts)
            .AsQueryable();

        // Apply all filters
        if (locationOptions.LocationId != null && locationOptions.LocationId is not 0)
        {
            query = query.Where(l => l.LocationId == locationOptions.LocationId);
        }

        if (!string.IsNullOrWhiteSpace(locationOptions.Name))
        {
            query = query.Where(l => EF.Functions.Like(l.Name, $"%{locationOptions.Name}%"));
        }
        if (!string.IsNullOrWhiteSpace(locationOptions.Address))
        {
            query = query.Where(l => EF.Functions.Like(l.Address, $"%{locationOptions.Address}%"));
        }
        if (!string.IsNullOrWhiteSpace(locationOptions.TownOrCity))
        {
            query = query.Where(l =>
                EF.Functions.Like(l.TownOrCity, $"%{locationOptions.TownOrCity}%")
            );
        }
        if (!string.IsNullOrWhiteSpace(locationOptions.StateOrCounty))
        {
            query = query.Where(l =>
                EF.Functions.Like(l.StateOrCounty, $"%{locationOptions.StateOrCounty}%")
            );
        }
		if (!string.IsNullOrWhiteSpace(locationOptions.ZipOrPostCode))
		{
			query = query.Where(l =>
				EF.Functions.Like(l.ZipOrPostCode , $"%{locationOptions.ZipOrPostCode}%")
			);
		}
		if (!string.IsNullOrWhiteSpace(locationOptions.Country))
		{
			query = query.Where(l =>
				EF.Functions.Like(l.Country , $"%{locationOptions.Country}%")
			);
		}
		// Simplified search implementation
		if (!string.IsNullOrWhiteSpace(locationOptions.Search))
        {
            query = query.Where(l =>
                l.LocationId.ToString().Contains(locationOptions.Search)
                || EF.Functions.Like(l.Name, $"%{locationOptions.Search}%")
               || EF.Functions.Like(l.Address, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.TownOrCity, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.StateOrCounty, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.ZipOrPostCode, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.Country, $"%{locationOptions.Search}%")
			);
        }

        if (!string.IsNullOrWhiteSpace(locationOptions.SortBy))
        {
            locationOptions.SortBy = locationOptions.SortBy.ToLowerInvariant();
            locationOptions.SortOrder = locationOptions.SortOrder?.ToLowerInvariant(); // Normalize sort order to lowercase
        }
        else
        {
            locationOptions.SortBy = "locationid"; // Default sort by LocationId if not specified
        }

        AnsiConsole.MarkupLine(
            $"[yellow]Applying sorting:[/] SortBy='{locationOptions.SortBy}', SortOrder='{locationOptions.SortOrder}'"
        );

        // Always apply sorting - whether SortBy is specified or not
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

		};

        AnsiConsole.MarkupLine("[yellow]Executing final query...[/]");

        // Execute query and get results
        var locations = (await query.ToListAsync()).Cast<Locations?>().ToList();

        if (locations.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No locations found with the specified criteria.[/]");
            return new ApiResponseDto<List<Locations?>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.NotFound,
                Message = "No locations found with the specified criteria.",
                Data = locations,
            };
        }

        AnsiConsole.MarkupLine(
            $"[green]Successfully retrieved {locations.Count} locations, sorted by '{locationOptions.SortBy}' in {locationOptions.SortOrder} order.[/]"
        );
        return new ApiResponseDto<List<Locations?>>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = "Locations retrieved successfully.",
            Data = locations,
        };
    }

	public async Task<ApiResponseDto<Locations?>> GetLocationById(int id)
	{
		Locations? location = await dbContext
			.Locations.Include(l => l.Workers)
			.Include(l => l.Shifts)
			.FirstOrDefaultAsync(l => l.LocationId == id);

		if (location is null)
		{
			return new ApiResponseDto<Locations?>
			{
				RequestFailed = true ,
				ResponseCode = System.Net.HttpStatusCode.NotFound ,
				Message = $"Location with ID: {id} not found." ,
				Data = null ,
			};
		}
		else
		{
			AnsiConsole.MarkupLine(
				$"[green]Successfully retrieved location with ID: {location.LocationId}.[/]"
			);
			return new ApiResponseDto<Locations?>
			{
				RequestFailed = false ,
				ResponseCode = System.Net.HttpStatusCode.OK ,
				Message = $"Location with ID: {id} retrieved successfully." ,
				Data = location ,
			};
		}
	}

	public async Task<ApiResponseDto<Locations>> CreateLocation(LocationApiRequestDto location)
    {
        try
        {
            Locations newLocation = new()
            {
                Name = location.Name ,
                Address = location.Address ,
                TownOrCity = location.TownOrCity ,
                StateOrCounty = location.StateOrCounty ,
                ZipOrPostCode = location.ZipOrPostCode ,
                Country = location.Country ,
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
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Back end location service - {ex}");
            return new ApiResponseDto<Locations>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "An error occurred while creating the location.",
                Data = null,
            };
        }
    }

    public async Task<ApiResponseDto<Locations?>> UpdateLocation(
        int id,
        LocationApiRequestDto updatedLocation
    )
    {
        Locations? savedLocation = await dbContext.Locations.FindAsync(id);

        if (savedLocation is null)
        {
            return new ApiResponseDto<Locations?>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Location with ID: {id} not found.",
            };
        }
        savedLocation.LocationId = id; // Ensure the LocationId is set to the ID being updated
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
        };
    }
}
