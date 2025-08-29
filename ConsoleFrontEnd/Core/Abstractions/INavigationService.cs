namespace ConsoleFrontEnd.Core.Abstractions;

/// <summary>
/// Navigation interface following Interface Segregation Principle
/// Handles menu navigation and state management
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the main menu
    /// </summary>
    Task NavigateToMainMenuAsync();

    /// <summary>
    /// Navigates to shift management
    /// </summary>
    Task NavigateToShiftManagementAsync();

    /// <summary>
    /// Navigates to location management
    /// </summary>
    Task NavigateToLocationManagementAsync();

    /// <summary>
    /// Navigates to worker management
    /// </summary>
    Task NavigateToWorkerManagementAsync();

    /// <summary>
    /// Exits the application safely
    /// </summary>
    Task ExitApplicationAsync();

    /// <summary>
    /// Gets the current navigation context
    /// </summary>
    string CurrentContext { get; }
}
