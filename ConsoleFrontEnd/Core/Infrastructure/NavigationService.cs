using ConsoleFrontEnd.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Core.Infrastructure;

/// <summary>
/// Navigation service implementation following Single Responsibility Principle
/// Manages application navigation state and flow
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IMenuFactory _menuFactory;
    private readonly ILogger<NavigationService> _logger;
    private readonly Stack<string> _navigationStack;
    private bool _shouldExit;

    public NavigationService(
        IMenuFactory menuFactory,
        ILogger<NavigationService> logger)
    {
        _menuFactory = menuFactory ?? throw new ArgumentNullException(nameof(menuFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _navigationStack = new Stack<string>();
    }

    public string CurrentContext => _navigationStack.Count > 0 ? 
        string.Join(" > ", _navigationStack.Reverse()) : "Application";

    public async Task NavigateToMainMenuAsync()
    {
        _navigationStack.Clear();
        _navigationStack.Push("Main Menu");
        _logger.LogDebug("Navigated to main menu");

        while (!_shouldExit)
        {
            try
            {
                var menu = _menuFactory.CreateMainMenu();
                await menu.DisplayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in main menu navigation");
                throw;
            }
        }
    }

    public async Task NavigateToShiftManagementAsync()
    {
        _navigationStack.Push("Shift Management");
        _logger.LogDebug("Navigated to shift management");

        try
        {
            var menu = _menuFactory.CreateShiftMenu();
            await menu.DisplayAsync();
        }
        finally
        {
            _navigationStack.Pop();
        }
    }

    public async Task NavigateToLocationManagementAsync()
    {
        _navigationStack.Push("Location Management");
        _logger.LogDebug("Navigated to location management");

        try
        {
            var menu = _menuFactory.CreateLocationMenu();
            await menu.DisplayAsync();
        }
        finally
        {
            _navigationStack.Pop();
        }
    }

    public async Task NavigateToWorkerManagementAsync()
    {
        _navigationStack.Push("Worker Management");
        _logger.LogDebug("Navigated to worker management");

        try
        {
            var menu = _menuFactory.CreateWorkerMenu();
            await menu.DisplayAsync();
        }
        finally
        {
            _navigationStack.Pop();
        }
    }

    public Task ExitApplicationAsync()
    {
        _logger.LogInformation("Application exit requested");
        _shouldExit = true;
        return Task.CompletedTask;
    }
}
