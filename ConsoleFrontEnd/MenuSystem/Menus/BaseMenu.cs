using ConsoleFrontEnd.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
/// Base menu implementation following Open/Closed Principle
/// Provides common functionality that can be extended by specific menus
/// </summary>
public abstract class BaseMenu : IMenu
{
    protected readonly IConsoleDisplayService DisplayService;
    protected readonly IConsoleInputService InputService;
    protected readonly INavigationService NavigationService;
    protected readonly ILogger Logger;

    protected BaseMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger logger)
    {
        DisplayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        InputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public abstract string Title { get; }
    public abstract string Context { get; }

    public virtual async Task DisplayAsync()
    {
        try
        {
            Logger.LogDebug("Displaying menu: {MenuTitle}", Title);
            
            DisplayService.DisplayHeader(Title);
            DisplayBreadcrumb();
            
            await ShowMenuAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error displaying menu: {MenuTitle}", Title);
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
            InputService.WaitForKeyPress();
            throw;
        }
    }

    /// <summary>
    /// Shows the specific menu implementation
    /// </summary>
    protected abstract Task ShowMenuAsync();

    /// <summary>
    /// Displays the navigation breadcrumb
    /// </summary>
    protected virtual void DisplayBreadcrumb()
    {
        DisplayService.DisplayInfo($"Navigation: {NavigationService.CurrentContext}");
        DisplayService.DisplayInfo(""); // Empty line for spacing
    }

    /// <summary>
    /// Handles common menu actions like going back or exiting
    /// </summary>
    protected virtual async Task<bool> HandleCommonActions(string choice)
    {
        switch (choice.ToLowerInvariant())
        {
            case "back":
            case "return to previous menu":
                return true; // Signal to exit current menu

            case "exit":
            case "exit application":
                await NavigationService.ExitApplicationAsync();
                return true;

            default:
                return false; // Not a common action, let specific menu handle it
        }
    }
}
