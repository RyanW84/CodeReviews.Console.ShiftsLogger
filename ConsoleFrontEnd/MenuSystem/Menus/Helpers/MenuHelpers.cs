using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public static class MenuHelpers
{
    public static void ShowWelcomeScreen()
    {
        AnsiConsole.Clear();
        var panel = new Panel(
                new FigletText("Shifts Logger")
                    .LeftJustified()
                    .Color(Color.Blue))
            .Header("[yellow]Welcome to[/]")
            .HeaderAlignment(Justify.Center)
            .Border(BoxBorder.Double)
            .BorderColor(Color.Yellow);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        var infoPanel = new Panel(
                "Manage your shifts, workers, and locations efficiently.\n" +
                "Navigate through the menus to perform various operations.")
            .Header("[green]System Information[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);

        AnsiConsole.Write(infoPanel);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    public static string GetMenuChoice(string title, params string[] choices)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{title}[/]")
                .AddChoices(choices)
                .HighlightStyle(new Style(Color.Blue, Color.Grey19))
        );
    }

    public static T GetUserInput<T>(string prompt, T? defaultValue = default) where T : struct
    {
        return AnsiConsole.Ask($"[green]{prompt}[/]", defaultValue ?? default(T));
    }

    public static string GetUserInput(string prompt, string? defaultValue = null)
    {
        return AnsiConsole.Ask<string>($"[green]{prompt}[/]", defaultValue ?? string.Empty);
    }

    public static T? GetOptionalUserInput<T>(string prompt, T? defaultValue = default) where T : struct
    {
        return AnsiConsole.Ask($"[green]{prompt}[/] [dim](or leave blank)[/]", defaultValue);
    }

    public static string? GetOptionalUserInput(string prompt, string? defaultValue = null)
    {
        return AnsiConsole.Ask<string?>($"[green]{prompt}[/] [dim](or leave blank)[/]", defaultValue);
    }

    public static void ShowBreadcrumb(params string[] path)
    {
        var breadcrumb = string.Join(" > ", path);
        AnsiConsole.MarkupLine($"[dim]{breadcrumb}[/]");
        AnsiConsole.WriteLine();
    }
}