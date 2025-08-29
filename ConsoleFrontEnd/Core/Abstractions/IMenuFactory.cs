namespace ConsoleFrontEnd.Core.Abstractions;

/// <summary>
/// Menu factory interface following Dependency Inversion Principle
/// Creates menu instances with proper dependency injection
/// </summary>
public interface IMenuFactory
{
    /// <summary>
    /// Creates the main menu
    /// </summary>
    IMenu CreateMainMenu();

    /// <summary>
    /// Creates the shift management menu
    /// </summary>
    IMenu CreateShiftMenu();

    /// <summary>
    /// Creates the location management menu
    /// </summary>
    IMenu CreateLocationMenu();

    /// <summary>
    /// Creates the worker management menu
    /// </summary>
    IMenu CreateWorkerMenu();
}
