namespace ConsoleFrontEnd.Core.Abstractions;

/// <summary>
/// Menu interface following Open/Closed Principle
/// Base contract for all menu implementations
/// </summary>
public interface IMenu
{
    /// <summary>
    /// Displays and handles the menu
    /// </summary>
    Task DisplayAsync();

    /// <summary>
    /// Gets the menu title
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the menu context path
    /// </summary>
    string Context { get; }
}
