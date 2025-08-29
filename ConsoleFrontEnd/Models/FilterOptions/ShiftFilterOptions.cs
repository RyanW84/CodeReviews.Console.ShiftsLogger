namespace ConsoleFrontEnd.Models.FilterOptions;

public class ShiftFilterOptions
{
    // This class defines the filter options for retrieving shifts, allowing filtering by worker ID, location ID, start time, and end time.
    public int? ShiftId { get; set; }
    public int? LocationId { get; set; }
    public int? WorkerId { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public DateTime? StartDate { get; set; } // Use DateTime for date-only filtering
    public DateTime? EndDate { get; set; } // Use DateTime for date-only filtering
    public string? Search { get; set; }

    // Linked table searching
    public string? LocationName { get; set; }
    public string? WorkerName { get; set; }

    // Duration filtering
    public int? MinDurationMinutes { get; set; } // Minimum duration in minutes
    public int? MaxDurationMinutes { get; set; } // Maximum duration in minutes

    // Sorting options
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}