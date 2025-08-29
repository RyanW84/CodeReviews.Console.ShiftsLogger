using ConsoleFrontEnd.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Core.Infrastructure;

/// <summary>
/// Main application implementation following Single Responsibility Principle
/// Coordinates the application lifecycle
/// </summary>
public class ConsoleApplication : IApplication
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<ConsoleApplication> _logger;

    public ConsoleApplication(
        INavigationService navigationService,
        ILogger<ConsoleApplication> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("Starting Console Application");
            
            // Navigate to main menu to start the application
            await _navigationService.NavigateToMainMenuAsync();
            
            _logger.LogInformation("Console Application ended gracefully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in console application");
            throw;
        }
    }
}
