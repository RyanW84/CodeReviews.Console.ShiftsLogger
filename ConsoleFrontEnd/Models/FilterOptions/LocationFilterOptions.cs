namespace ConsoleFrontEnd.Models.FilterOptions;

public class LocationFilterOptions
{
    // This class defines the filter options for retrieving locations, allowing filtering by location ID and name.
    public int? LocationId { get; set; } = 0; // Optional filter by location ID
    public string? Name { get; set; } // Optional filter by location name
    public string? Description { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty; // Optional filter by location address
    public string? Town { get; set; } = string.Empty; // Optional filter by Town
    public string? County { get; set; } = string.Empty; // Optional filter by County
    public string? PostCode { get; set; } = string.Empty; // Optional filter by Postcode
    public string? Country { get; set; } = string.Empty; // Optional filter by Country
    public string? Search { get; set; } = string.Empty; // Optional search text for filtering locations
    public string? SortBy { get; set; } = "Name"; // Default sorting by name
    public string? SortOrder { get; set; } = "asc"; // Default sorting order is ascending
}