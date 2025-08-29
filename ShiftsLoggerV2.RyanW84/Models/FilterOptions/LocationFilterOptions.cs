using Microsoft.AspNetCore.Mvc;

namespace ShiftsLoggerV2.RyanW84.Models.FilterOptions;

public class LocationFilterOptions : BaseFilterOptions
{
    // This class defines the filter options for retrieving locations, allowing filtering by location ID and name.
    [FromQuery(Name = "LocationId")] public int? LocationId { get; set; } = 0;

    [FromQuery(Name = "Name")] public string Name { get; set; } = string.Empty;

    [FromQuery(Name = "Address")] public string Address { get; set; } = string.Empty;

    [FromQuery(Name = "Town")] public string Town { get; set; } = string.Empty;

    [FromQuery(Name = "County")] public string County { get; set; } = string.Empty;

    [FromQuery(Name = "State")]
    public string State { get; set; } = string.Empty;

    [FromQuery(Name = "PostCode")]
    public string PostCode { get; set; } = string.Empty;

    [FromQuery(Name = "Country")] public string Country { get; set; } = string.Empty;

    [FromQuery(Name = "SortBy")] public string SortBy { get; set; } = "LocationId";

    [FromQuery(Name = "SortOrder")] public string SortOrder { get; set; } = "ASC";

    [FromQuery(Name = "Search")] public string Search { get; set; } = string.Empty;
}