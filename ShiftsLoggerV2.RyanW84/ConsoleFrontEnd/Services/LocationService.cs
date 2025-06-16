using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.Services;

public class LocationService : ILocationService
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7009/"),
    };

    public async Task<ApiResponseDto<List<Locations>>> GetAllLocations(LocationFilterOptions locationFilterOptions)
    {
        try
        {
            PrintFilterOptions(locationFilterOptions);

            var queryString = BuildQueryString("api/locations", locationFilterOptions);

            AnsiConsole.MarkupLine($"[blue]Final request URL: {httpClient.BaseAddress}{queryString}[/]\n");

            var response = await httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                AnsiConsole.Markup("[red]Locations not retrieved.[/]\n");
                return new ApiResponseDto<List<Locations>>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Unknown error",
                    Data = null,
                };
            }

            AnsiConsole.Markup("[green]Locations retrieved successfully.[/]\n");
            return await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Locations>>>()
                ?? new ApiResponseDto<List<Locations>>
                {
                    ResponseCode = response.StatusCode,
                    Message = "Data obtained",
                    Data = new List<Locations>(),
                };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetAllLocations: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<List<Locations?>>> GetLocationById(int id)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/locations/{id}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                AnsiConsole.Markup("[red]Error: Location not found[/]\n");
                return new ApiResponseDto<List<Locations>>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Not found",
                    Data = null,
                };
            }

            AnsiConsole.Markup("[green]Location retrieved successfully.[/]\n");
            return await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Locations>>>()
                ?? new ApiResponseDto<List<Locations>>
                {
                    ResponseCode = response.StatusCode,
                    Message = "Location found",
                    Data = response.Content.ReadFromJsonAsync<List<Locations>>().Result ?? new List<Locations>() ,
					TotalCount = 0,
                };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetLocationById: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Locations>> CreateLocation(Locations createdLocation)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/locations", createdLocation);
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                Console.WriteLine($"Error: Status Code - {response.StatusCode}");
                return new ApiResponseDto<Locations>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Creation failed",
                    Data = null,
                };
            }

            Console.WriteLine("Location created successfully.");
            return new ApiResponseDto<Locations>
            {
                ResponseCode = response.StatusCode,
                Data = await response.Content.ReadFromJsonAsync<Locations>() ?? createdLocation,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CreateLocation: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Locations?>> UpdateLocation(int id, Locations updatedLocation)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/locations/{id}", updatedLocation);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                return new ApiResponseDto<Locations>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Update failed",
                    Data = null,
                };
            }

            AnsiConsole.Markup("[green]Location updated successfully.[/]\n");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            Console.Clear();
            return new ApiResponseDto<Locations>
            {
                ResponseCode = response.StatusCode,
                Data = await response.Content.ReadFromJsonAsync<Locations>() ?? updatedLocation,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for UpdateLocation: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<string?>> DeleteLocation(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/locations/{id}");
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                AnsiConsole.Markup("[red]Error: Location not found please try again![/]\n");
                return new ApiResponseDto<string>
                {
                    ResponseCode = response.StatusCode,
                    Message = $"Error: {response.StatusCode}",
                    Data = null,
                };
            }

            AnsiConsole.Markup("[green]Location deleted successfully![/]");
            return new ApiResponseDto<string>
            {
                ResponseCode = response.StatusCode,
                Message = response.ReasonPhrase,
                Data = null,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for DeleteLocation: {ex}");
            throw;
        }
    }

    private static void PrintFilterOptions(LocationFilterOptions options)
    {
        AnsiConsole.MarkupLine(
            $"[yellow]Filter options received:[/]\n" +
            $"[blue]LocationId:[/] {options.LocationId}\t" +
            $"[blue]Name:[/] {options.Name}\t" +
            $"[blue]Address:[/] {options.Address}\t" +
            $"[blue]TownOrCity:[/] {options.TownOrCity}\t" +
            $"[blue]StateOrCounty:[/] {options.StateOrCounty}\t" +
            $"[blue]ZipOrPostCode:[/] {options.ZipOrPostCode}\t" +
            $"[blue]Country:[/] {options.Country}\t" +
            $"[blue]Search:[/] {options.Search}\t" +
            $"[blue]SortBy:[/] {options.SortBy}\t" +
            $"[blue]SortOrder:[/] {options.SortOrder}"
        );
    }

    private static string BuildQueryString(string basePath, LocationFilterOptions options)
    {
        var queryParams = new List<string>();

        void AddIfNotNullOrEmpty(string key, object? value)
        {
            switch (value)
            {
                case int intValue:
                    queryParams.Add($"{key}={intValue}");
                    break;
                case string strValue when !string.IsNullOrWhiteSpace(strValue):
                    queryParams.Add($"{key}={strValue}");
                    break;
            }
        }

        AddIfNotNullOrEmpty("locationId", options.LocationId);
        AddIfNotNullOrEmpty("name", options.Name);
        AddIfNotNullOrEmpty("address", options.Address);
        AddIfNotNullOrEmpty("townOrCity", options.TownOrCity);
        AddIfNotNullOrEmpty("stateOrCounty", options.StateOrCounty);
        AddIfNotNullOrEmpty("zipOrPostCode", options.ZipOrPostCode);
        AddIfNotNullOrEmpty("country", options.Country);
        AddIfNotNullOrEmpty("search", options.Search);
        AddIfNotNullOrEmpty("sortBy", options.SortBy);
        AddIfNotNullOrEmpty("sortOrder", options.SortOrder);

        return queryParams.Count > 0
            ? $"{basePath}?{string.Join("&", queryParams)}"
            : basePath;
    }
}
