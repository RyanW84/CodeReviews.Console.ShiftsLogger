using ConsoleFrontEnd.Core.Abstractions;
using Spectre.Console;

namespace ConsoleFrontEnd.Core.Infrastructure;

/// <summary>
/// Console input service implementation using Spectre.Console
/// Following Single Responsibility Principle - handles only input operations
/// </summary>
public class SpectreConsoleInputService : IConsoleInputService
{
    private static readonly object _consoleLock = new();

    public string GetMenuChoice(string prompt, params string[] options)
    {
        lock (_consoleLock)
        {
            if (options == null || options.Length == 0)
                throw new ArgumentException("At least one option must be provided", nameof(options));

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[green]{prompt}[/]")
                    .PageSize(10)
                    .AddChoices(options));

            return selection;
        }
    }

    public string GetTextInput(string prompt, bool isRequired = true)
    {
        lock (_consoleLock)
        {
            var textPrompt = new TextPrompt<string>($"[green]{prompt}[/]");
            
            if (!isRequired)
            {
                textPrompt.AllowEmpty();
            }

            return AnsiConsole.Prompt(textPrompt);
        }
    }

    public int GetIntegerInput(string prompt, int? min = null, int? max = null)
    {
        lock (_consoleLock)
        {
            var numberPrompt = new TextPrompt<int>($"[green]{prompt}[/]")
                .ValidationErrorMessage("[red]Please enter a valid integer[/]");

            if (min.HasValue)
                numberPrompt.Validate(value => value >= min.Value ? ValidationResult.Success() 
                    : ValidationResult.Error($"Value must be at least {min.Value}"));

            if (max.HasValue)
                numberPrompt.Validate(value => value <= max.Value ? ValidationResult.Success() 
                    : ValidationResult.Error($"Value must be at most {max.Value}"));

            return AnsiConsole.Prompt(numberPrompt);
        }
    }

    public decimal GetDecimalInput(string prompt, decimal? min = null, decimal? max = null)
    {
        lock (_consoleLock)
        {
            var decimalPrompt = new TextPrompt<decimal>($"[green]{prompt}[/]")
                .ValidationErrorMessage("[red]Please enter a valid decimal number[/]");

            if (min.HasValue)
                decimalPrompt.Validate(value => value >= min.Value ? ValidationResult.Success() 
                    : ValidationResult.Error($"Value must be at least {min.Value}"));

            if (max.HasValue)
                decimalPrompt.Validate(value => value <= max.Value ? ValidationResult.Success() 
                    : ValidationResult.Error($"Value must be at most {max.Value}"));

            return AnsiConsole.Prompt(decimalPrompt);
        }
    }

    public DateTime GetDateTimeInput(string prompt)
    {
        lock (_consoleLock)
        {
            var datePrompt = new TextPrompt<DateTime>($"[green]{prompt}[/]")
                .ValidationErrorMessage("[red]Please enter a valid date and time (e.g., 2023-12-31 14:30)[/]");

            return AnsiConsole.Prompt(datePrompt);
        }
    }

    public bool GetConfirmation(string prompt)
    {
        lock (_consoleLock)
        {
            return AnsiConsole.Confirm($"[yellow]{prompt}[/]");
        }
    }

    public void WaitForKeyPress(string message = "Press any key to continue...")
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[dim]{Markup.Escape(message)}[/]");
            Console.ReadKey(true);
        }
    }
}
