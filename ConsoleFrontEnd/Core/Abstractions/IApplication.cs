namespace ConsoleFrontEnd.Core.Abstractions;

/// <summary>
/// Main application interface following Single Responsibility Principle
/// Defines the contract for the application entry point
/// </summary>
public interface IApplication
{
    /// <summary>
    /// Runs the console application
    /// </summary>
    /// <returns>Task representing the application execution</returns>
    Task RunAsync();
}
