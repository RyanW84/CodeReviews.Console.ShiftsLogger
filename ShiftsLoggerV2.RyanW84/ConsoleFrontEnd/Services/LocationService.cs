using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.Services;

public class LocationService : ILocationService
{
    private readonly HttpClient httpClient = new HttpClient()
    {
        BaseAddress = new Uri("https://localhost:7009/"),
    };

    public async Task<ApiResponseDto<List<Locations>>> GetAllLocations(
        LocationFilterOptions locationFilterOptions
    )
    {
        HttpResponseMessage response;
        try
        {
            // Debug log for incoming search parameter
            AnsiConsole.MarkupLine(
                $"[yellow]Filter options received:[/]\n\n"
                    + $"[blue]LocationId:[/] {locationFilterOptions.LocationId}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.Name}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.Address}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.TownOrCity}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.StateOrCounty}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.ZipOrPostCode}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.Country}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.Search}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.SortBy}\t"
                    + $"[blue]LocationId:[/] {locationFilterOptions.SortOrder}\t"
            );

            var queryParams = new List<string>();
            if (locationFilterOptions.LocationId != null)
                queryParams.Add($"locationId={locationFilterOptions.LocationId}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.Name))
                queryParams.Add($"name={locationFilterOptions.Name}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.Address))
                queryParams.Add($"name={locationFilterOptions.Address}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.TownOrCity))
                queryParams.Add($"name={locationFilterOptions.TownOrCity}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.StateOrCounty))
                queryParams.Add($"name={locationFilterOptions.StateOrCounty}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.ZipOrPostCode))
                queryParams.Add($"name={locationFilterOptions.ZipOrPostCode}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.Country))
                queryParams.Add($"name={locationFilterOptions.Country}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.Search))
                queryParams.Add($"search={locationFilterOptions.Search}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.SortBy))
                queryParams.Add($"sortBy={locationFilterOptions.SortBy}");
            if (!string.IsNullOrWhiteSpace(locationFilterOptions.SortOrder))
                queryParams.Add($"sortOrder={locationFilterOptions.SortOrder}");

            var queryString = "api/locations";
            if (queryParams.Count > 0)
                queryString += "?" + string.Join("&", queryParams);

            AnsiConsole.MarkupLine(
                $"[blue]Final request URL: {httpClient.BaseAddress}{queryString}[/]\n"
            );

            response = await httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                AnsiConsole.Markup("[Red]Locations not retrieved.[/]\n");
                return new ApiResponseDto<List<Locations>>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase,
                    Data = null,
                };
            }
            else
            {
                AnsiConsole.Markup("[Green]Locations retrieved successfully.[/]\n");
                var locations =
                    await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Locations>>>()
                    ?? new ApiResponseDto<List<Locations>>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "Data obtained",
                        Data = new List<Locations>(),
                    };

                return locations;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetAllLocations: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<List<Locations?>>> GetLocationById(int id)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"api/locations/{id}");

            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                AnsiConsole.Markup($"[Red]Error: Location not found[/]\n");
                return new ApiResponseDto<List<Locations>>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase,
                    Data = null,
                };
            }
            else
            {
                AnsiConsole.Markup("[Green]Location retrieved successfully.[/]\n");
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Locations>>>()
                    ?? new ApiResponseDto<List<Locations>>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "Location found",
                        Data = new List<Locations>(),
                        TotalCount = 0,
                    };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetLocationById: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Locations>> CreateLocation(Locations createdLocation)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/locations", createdLocation);
            if (response.StatusCode is not System.Net.HttpStatusCode.Created)
            {
                Console.WriteLine($"Error: Status Code - {response.StatusCode}");
                return new ApiResponseDto<Locations>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase,
                    Data = null,
                };
            }
            else
            {
                Console.WriteLine("Location created successfully.");
                return new ApiResponseDto<Locations>
                {
                    ResponseCode = response.StatusCode,
                    Data =
                        response.Content.ReadFromJsonAsync<Locations>().Result ?? createdLocation,
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CreateLocation: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Locations?>> UpdateLocation(int id, Locations updatedLocation)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"api/locations/{id}", updatedLocation);
            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                return new ApiResponseDto<Locations>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase,
                    Data = null,
                };
            }
            else
            {
                AnsiConsole.Markup("[Green]Location updated successfully.[/]\n");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                Console.Clear();
                return new ApiResponseDto<Locations>
                {
                    ResponseCode = response.StatusCode,
                    Data =
                        response.Content.ReadFromJsonAsync<Locations>().Result ?? updatedLocation,
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for UpdateLocation: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<string?>> DeleteLocation(int id)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync($"api/locations/{id}");
            if (response.StatusCode is not System.Net.HttpStatusCode.NoContent)
            {
                AnsiConsole.Markup("[red]Error: Location not found please try again![/]\n");
                return new ApiResponseDto<string>
                {
                    ResponseCode = response.StatusCode,
                    Message = $"Error: {response.StatusCode}",
                    Data = null,
                };
            }
            else
            {
                AnsiConsole.Markup("[green]Location deleted successfully![/]");
                return new ApiResponseDto<string>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase,
                    Data = null,
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for DeleteLocation: {ex}");
            throw;
        }
    }
}
