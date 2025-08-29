using ConsoleFrontEnd.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
/// Main menu implementation following Single Responsibility Principle
/// Handles main application navigation
/// </summary>
public class MainMenu : BaseMenu
{
    private static bool _firstRun = true;

    public MainMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<MainMenu> logger)
        : base(displayService, inputService, navigationService, logger)
    {
    }

    public override string Title => "Shifts Logger - Main Menu";
    public override string Context => "Main Menu";

    protected override async Task ShowMenuAsync()
    {
        if (_firstRun)
        {
            ShowWelcomeMessage();
            _firstRun = false;
        }

        var choice = InputService.GetMenuChoice(
            "Select an option:",
            "Shift Management",
            "Location Management", 
            "Worker Management",
            "System Information",
            "Exit Application"
        );

        await HandleMenuChoice(choice);
    }

    private async Task HandleMenuChoice(string choice)
    {
        Logger.LogDebug("Main menu choice selected: {Choice}", choice);

        if (await HandleCommonActions(choice))
            return;

        switch (choice)
        {
            case "Shift Management":
                await NavigationService.NavigateToShiftManagementAsync();
                break;

            case "Location Management":
                await NavigationService.NavigateToLocationManagementAsync();
                break;

            case "Worker Management":
                await NavigationService.NavigateToWorkerManagementAsync();
                break;

            case "System Information":
                ShowSystemInformation();
                break;

            default:
                DisplayService.DisplayError("Invalid menu choice");
                InputService.WaitForKeyPress();
                break;
        }
    }

    private void ShowWelcomeMessage()
    {
        DisplayService.DisplaySuccess("Welcome to Shifts Logger Console Application!");
        DisplayService.DisplayInfo("Manage your shifts, locations, and workers efficiently.");
        DisplayService.DisplayInfo(""); // Empty line for spacing
    }

    private void ShowSystemInformation()
    {
        DisplayService.DisplayHeader("System Information", "cyan");
        DisplayService.DisplaySystemInfo();
        InputService.WaitForKeyPress();
    }
}
