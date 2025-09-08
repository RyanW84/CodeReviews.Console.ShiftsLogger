namespace ConsoleFrontEnd.Core.Abstractions;

/// <summary>
/// Input service interface following Interface Segregation Principle
/// Handles all user input operations
/// </summary>
public interface IConsoleInputService
{
    /// <summary>
    /// Gets a menu choice from the user
    /// </summary>
    string GetMenuChoice(string prompt, params string[] options);

    /// <summary>
    /// Gets text input from the user
    /// </summary>
    string GetTextInput(string prompt, bool isRequired = true);

    /// <summary>
    /// Gets integer input from the user
    /// </summary>
    int GetIntegerInput(string prompt, int? min = null, int? max = null);

    /// <summary>
    /// Gets decimal input from the user
    /// </summary>
    decimal GetDecimalInput(string prompt, decimal? min = null, decimal? max = null);

    /// <summary>
    /// Gets DateTime input from the user
    /// </summary>
    DateTime GetDateTimeInput(string prompt);

    /// <summary>
    /// Gets confirmation from the user
    /// </summary>
    bool GetConfirmation(string prompt);

    /// <summary>
    /// Gets a menu choice from the user (async)
    /// </summary>
    Task<string> GetMenuChoiceAsync(string prompt, params string[] options);

    /// <summary>
    /// Gets text input from the user (async)
    /// </summary>
    Task<string> GetTextInputAsync(string prompt, bool isRequired = true);

    /// <summary>
    /// Gets integer input from the user (async)
    /// </summary>
    Task<int> GetIntegerInputAsync(string prompt, int? min = null, int? max = null);

    /// <summary>
    /// Gets decimal input from the user (async)
    /// </summary>
    Task<decimal> GetDecimalInputAsync(string prompt, decimal? min = null, decimal? max = null);

    /// <summary>
    /// Gets DateTime input from the user (async)
    /// </summary>
    Task<DateTime> GetDateTimeInputAsync(string prompt);

    /// <summary>
    /// Gets confirmation from the user (async)
    /// </summary>
    Task<bool> GetConfirmationAsync(string prompt);

    /// <summary>
    /// Waits for user to press any key (async)
    /// </summary>
    Task WaitForKeyPressAsync(string message = "Press any key to continue...");
}
