using ConsoleFrontEnd.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem.Common;

/// <summary>
/// Common UI helper operations following SOLID principles and DRY
/// Provides reusable UI operations to reduce code duplication in UI classes
/// </summary>
public class UiHelper
{
    private readonly IConsoleDisplayService _display;
    private readonly ILogger _logger;

    public UiHelper(IConsoleDisplayService display, ILogger logger)
    {
        _display = display ?? throw new ArgumentNullException(nameof(display));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Display header for create operations
    /// </summary>
    public void DisplayCreateHeader(string entityName)
    {
        _display.DisplayHeader($"Create New {entityName}");
    }

    /// <summary>
    /// Display header for update operations
    /// </summary>
    public void DisplayUpdateHeader(string entityName, string identifier)
    {
        _display.DisplayHeader($"Update {entityName}: {identifier}");
    }

    /// <summary>
    /// Display header for filter operations
    /// </summary>
    public void DisplayFilterHeader(string entityPluralName)
    {
        _display.DisplayHeader($"Filter {entityPluralName}");
    }

    /// <summary>
    /// Get required string input with validation
    /// </summary>
    public string GetRequiredStringInput(string prompt)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>($"[green]{prompt}:[/]");
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();

            DisplayValidationError("This field is required.");
        }
    }

    /// <summary>
    /// Get optional string input
    /// </summary>
    public string? GetOptionalStringInput(string prompt, string? defaultValue = null)
    {
        var fullPrompt = $"[yellow]{prompt} (or press Enter to skip):[/]";
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>(fullPrompt)
                .AllowEmpty()
                .DefaultValue(defaultValue ?? string.Empty)
        );
        return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
    }

    /// <summary>
    /// Get optional typed input with error handling
    /// </summary>
    public TResult? GetOptionalInput<TResult>(string prompt, TResult? defaultValue = default)
    {
        try
        {
            return AnsiConsole.Ask<TResult?>($"[yellow]{prompt} (press Enter to skip):[/]", defaultValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting input for prompt: {Prompt}", prompt);
            return defaultValue;
        }
    }

    /// <summary>
    /// Get optional DateTime input with validation
    /// </summary>
    public DateTime? GetOptionalDateTimeInput(string prompt)
    {
        try
        {
            // Use a more explicit prompt to make it clear that Enter skips
            var fullPrompt = $"[yellow]{prompt} (format: dd/MM/yyyy HH:mm, or press Enter to skip):[/]";
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>(fullPrompt)
                    .AllowEmpty()
                    .DefaultValue(string.Empty)
            );
            
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (DateTime.TryParseExact(input, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var result))
                return result;

            DisplayValidationError("Invalid date format. Please use dd/MM/yyyy HH:mm");
            return GetOptionalDateTimeInput(prompt); // Retry
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting DateTime input for prompt: {Prompt}", prompt);
            return null;
        }
    }

    /// <summary>
    /// Get optional integer input with validation
    /// </summary>
    public int? GetOptionalIntInput(string prompt)
    {
        try
        {
            var fullPrompt = $"[yellow]{prompt} (or press Enter to skip):[/]";
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>(fullPrompt)
                    .AllowEmpty()
                    .DefaultValue(string.Empty)
            );
            
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (int.TryParse(input, out var result))
                return result;

            DisplayValidationError("Invalid number format. Please enter a valid integer.");
            return GetOptionalIntInput(prompt); // Retry
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting integer input for prompt: {Prompt}", prompt);
            return null;
        }
    }

    /// <summary>
    /// Get DateTime input with validation
    /// </summary>
    public DateTime GetRequiredDateTimeInput(string prompt)
    {
        while (true)
        {
            try
            {
                var input = AnsiConsole.Ask<string>($"[green]{prompt} (dd/MM/yyyy HH:mm):[/]");
                if (DateTime.TryParseExact(input, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var result))
                    return result;
                DisplayValidationError("Invalid date format. Please use dd/MM/yyyy HH:mm");
            }
            catch
            {
                DisplayValidationError("Invalid date format. Please use dd/MM/yyyy HH:mm");
            }
        }
    }

    /// <summary>
    /// Get integer input with validation
    /// </summary>
    public int GetRequiredIntInput(string prompt)
    {
        while (true)
        {
            try
            {
                return AnsiConsole.Ask<int>($"[green]{prompt}:[/]");
            }
            catch
            {
                DisplayValidationError("Please enter a valid number.");
            }
        }
    }

    /// <summary>
    /// Display validation error
    /// </summary>
    public void DisplayValidationError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    public bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
    }

    /// <summary>
    /// Display entities table
    /// </summary>
    public void DisplayEntitiesTable<T>(IEnumerable<T> entities, string title)
    {
        _display.DisplayTable(entities, title);
    }

    /// <summary>
    /// Get entity ID input
    /// </summary>
    public int GetEntityIdInput(string entityName)
    {
        return GetRequiredIntInput($"Enter {entityName.ToLower()} ID");
    }

    /// <summary>
    /// Select entity ID
    /// </summary>
    public int SelectEntityInput(string entityName)
    {
        return GetRequiredIntInput($"Select {entityName.ToLower()} ID");
    }

    /// <summary>
    /// Extract count from menu choice string in format "count. Name"
    /// </summary>
    public static int ExtractCountFromChoice(string choice)
    {
        var span = choice.AsSpan();
        var dotIndex = span.IndexOf('.');
        if (dotIndex == -1)
            throw new ArgumentException("Invalid choice format. Expected 'count. Name' format.");
        
        return int.Parse(span[..dotIndex]);
    }
}
