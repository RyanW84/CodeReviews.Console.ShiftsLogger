using ConsoleFrontEnd.Models;

namespace ConsoleFrontEnd.Services.Common;

/// <summary>
/// Data structure for displaying shifts with pagination information
/// </summary>
public class ShiftDisplayData
{
    public List<ShiftDisplayItem> Shifts { get; set; } = new();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasMultiplePages => TotalPages > 1;
}

/// <summary>
/// Individual shift display item with computed display properties
/// </summary>
public class ShiftDisplayItem
{
    public int DisplayIndex { get; set; }
    public Shift Shift { get; set; } = null!;
    public string FormattedDuration { get; set; } = string.Empty;
    public string FormattedStartTime { get; set; } = string.Empty;
    public string FormattedEndTime { get; set; } = string.Empty;
}
