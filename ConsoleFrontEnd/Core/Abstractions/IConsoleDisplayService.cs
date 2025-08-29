namespace ConsoleFrontEnd.Core.Abstractions;

/// <summary>
/// Display service interface following Interface Segregation Principle
/// Handles all console output operations
/// </summary>
public interface IConsoleDisplayService
{
    /// <summary>
    /// Displays a header with styling
    /// </summary>
    void DisplayHeader(string title, string color = "yellow");

    /// <summary>
    /// Displays an error message
    /// </summary>
    void DisplayError(string message);

    /// <summary>
    /// Displays a success message
    /// </summary>
    void DisplaySuccess(string message);

    /// <summary>
    /// Displays an info message
    /// </summary>
    void DisplayInfo(string message);

    /// <summary>
    /// Displays a warning message
    /// </summary>
    void DisplayWarning(string message);

    /// <summary>
    /// Clears the console
    /// </summary>
    void Clear();

    /// <summary>
    /// Displays a table with data
    /// </summary>
    void DisplayTable<T>(IEnumerable<T> data, string? title = null, int startingRowNumber = 1);

    /// <summary>
    /// Displays system information
    /// </summary>
    void DisplaySystemInfo();
}
