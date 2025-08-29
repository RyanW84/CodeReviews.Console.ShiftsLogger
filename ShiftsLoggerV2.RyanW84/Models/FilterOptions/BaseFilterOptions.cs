using Microsoft.AspNetCore.Mvc;

namespace ShiftsLoggerV2.RyanW84.Models.FilterOptions;

public abstract class BaseFilterOptions
{
    [FromQuery(Name = "pageNumber")] public int PageNumber { get; set; } = 1;

    [FromQuery(Name = "pageSize")] public int PageSize { get; set; } = 10;

    // Validation for pagination parameters
    public void ValidatePagination()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 50;
        if (PageSize > 1000) PageSize = 1000; // Max page size to prevent abuse
    }
}
