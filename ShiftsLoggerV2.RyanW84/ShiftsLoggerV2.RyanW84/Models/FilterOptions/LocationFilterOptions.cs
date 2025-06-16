using Microsoft.AspNetCore.Mvc;

namespace ShiftsLoggerV2.RyanW84.Models.FilterOptions;

public class LocationFilterOptions
{
    // This class defines the filter options for retrieving locations, allowing filtering by location ID and name.
    [FromQuery(Name = "LocationId")]
    public int? LocationId { get; set; } = 0;

    [FromQuery(Name = "Name")]
    public string Name { get; set; } = string.Empty; // Use string for name filtering

    [FromQuery(Name = "Address")]
    public string Address { get; set; } = string.Empty; // Use string for phone filtering

    [FromQuery(Name = "TownOrCity")]
    public string TownOrCity { get; set; } = string.Empty; // Use string for email filtering

    [FromQuery(Name = "StateOrCounty")]
    public string StateOrCounty { get; set; } = string.Empty; // Use string for email filtering

    [FromQuery(Name = "ZipOrPostCode")]
    public string ZipOrPostCode { get; set; } = string.Empty; // Use string for email filtering

    [FromQuery(Name = "Country")]
    public string Country { get; set; } = string.Empty; // Use string for email filtering

    [FromQuery(Name = "SortBy")]
    public string SortBy { get; set; } = "LocationId"; // Use string for sorting options

    [FromQuery(Name = "SortOrder")]
    public string SortOrder { get; set; } = "ASC"; // Use string for sorting options

    [FromQuery(Name = "Search")]
    public string Search { get; set; } = string.Empty; // Use string for search options
}
