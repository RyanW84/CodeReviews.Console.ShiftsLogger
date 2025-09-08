using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Services.Common;
using Spectre.Console;

namespace ConsoleFrontEnd.Services.Display;

/// <summary>
/// Interface for shift display operations
/// Follows Single Responsibility Principle by handling display logic
/// </summary>
public interface IShiftDisplayService
{
    /// <summary>
    /// Displays a table of shifts with pagination information
    /// </summary>
    Task DisplayShiftsTableWithPaginationAsync(List<Shift> shifts, int pageNumber, int pageSize, int totalCount);

    /// <summary>
    /// Displays detailed information for a single shift
    /// </summary>
    Task DisplayShiftDetailsAsync(Shift shift);

    /// <summary>
    /// Creates and returns a Spectre.Console table for shifts
    /// </summary>
    Table CreateShiftsTable(IEnumerable<ShiftDisplayItem> displayItems);
}
