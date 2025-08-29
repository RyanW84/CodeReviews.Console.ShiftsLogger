using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public static class NavigationManager
{
    private static readonly Stack<string> _navigationHistory = new();

    public static void PushToHistory(string menuName)
    {
        _navigationHistory.Push(menuName);
    }

    public static string PopFromHistory()
    {
        return _navigationHistory.Count > 0 ? _navigationHistory.Pop() : "Main Menu";
    }

    public static void ClearHistory()
    {
        _navigationHistory.Clear();
    }

    public static string GetCurrentPath()
    {
        if (_navigationHistory.Count == 0)
            return "Main Menu";

        return string.Join(" > ", _navigationHistory.Reverse());
    }

    public static void ShowNavigationPath()
    {
        var path = GetCurrentPath();
        AnsiConsole.MarkupLine($"[dim]Location: {path}[/]");
        AnsiConsole.WriteLine();
    }

    public static bool ConfirmExit(string currentMenu)
    {
        return AnsiConsole.Confirm($"Are you sure you want to exit {currentMenu}?");
    }
}