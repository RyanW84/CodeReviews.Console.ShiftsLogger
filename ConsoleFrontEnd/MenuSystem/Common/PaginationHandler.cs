using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleFrontEnd.MenuSystem.Common;

public class PaginationHandler
{
    private readonly IConsoleDisplayService _display;
    private readonly IConsoleInputService _input;

    public PaginationHandler(IConsoleDisplayService display, IConsoleInputService input)
    {
        _display = display;
        _input = input;
    }

    public async Task<object?> HandlePaginationAsync<T>(
        Func<int, int, Task<ApiResponseDto<List<T>>>> loadPageFunc,
        Action<ApiResponseDto<List<T>>, int, int> displayPageFunc,
        Func<ApiResponseDto<List<T>>, int, int, List<string>>? buildChoicesFunc,
        Func<string, ApiResponseDto<List<T>>, int, int, Task<object?>>? handleSelectionFunc,
        int initialPage = 1,
        int pageSize = 10
    )
    {
        var currentPage = initialPage;
        bool isSelectionMode = buildChoicesFunc != null && handleSelectionFunc != null;

        while (true)
        {
            var response = await loadPageFunc(currentPage, pageSize);
            if (response == null || response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (response != null) HandleEmptyPage(response, currentPage);
                else _display.DisplayError("Failed to load page.");
                if (isSelectionMode && currentPage == 1) return await _input.GetIntegerInputAsync("[green]Enter ID:[/]") as object;
                return null;
            }

            displayPageFunc(response, currentPage, pageSize);

            if (isSelectionMode)
            {
                var choices = buildChoicesFunc!(response, currentPage, pageSize);
                var selected = AnsiConsole.Prompt(new SelectionPrompt<string>().Title($"Select Item (Page {response.PageNumber} of {response.TotalPages}):").AddChoices(choices));

                var result = await handleSelectionFunc!(selected, response, currentPage, pageSize);
                if (result != null) return result;
                // If result is null, continue pagination
            }
            else
            {
                var choice = GetPaginationChoice(response);
                var (shouldContinue, newPage, newSize) = await HandleChoiceAsync(choice, currentPage, pageSize, response);
                if (!shouldContinue) return null;
                currentPage = newPage;
                pageSize = newSize;
            }
        }
    }

    private string GetPaginationChoice<T>(ApiResponseDto<List<T>> response)
    {
        var options = new List<string>();
        if (response.HasPreviousPage) options.Add("Previous Page");
        if (response.HasNextPage) options.Add("Next Page");
        options.AddRange(new[] { "Go to Page", "Change Page Size", "Back to Menu" });
        return AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Choose an action:").AddChoices(options));
    }

    private async Task<(bool, int, int)> HandleChoiceAsync<T>(string choice, int currentPage, int pageSize, ApiResponseDto<List<T>> response)
    {
        switch (choice)
        {
            case "Previous Page": return (true, currentPage - 1, pageSize);
            case "Next Page": return (true, currentPage + 1, pageSize);
            case "Go to Page": return (true, await HandleGoToPageAsync(response), pageSize);
            case "Change Page Size": return (true, 1, await HandleChangePageSizeAsync(pageSize));
            default: return (false, currentPage, pageSize);
        }
    }

    private async Task<int> HandleGoToPageAsync<T>(ApiResponseDto<List<T>> response)
    {
        var page = await _input.GetIntegerInputAsync($"Enter page number (1-{response.TotalPages}):", 1, response.TotalPages);
        return page >= 1 && page <= response.TotalPages ? page : response.PageNumber;
    }

    private async Task<int> HandleChangePageSizeAsync(int currentSize)
    {
        var size = await _input.GetIntegerInputAsync("Enter new page size (1-100):", 1, 100);
        return size >= 1 && size <= 100 ? size : currentSize;
    }

    private void HandleEmptyPage<T>(ApiResponseDto<List<T>> response, int pageNumber)
    {
        if (pageNumber == 1) _display.DisplayError("No items found.");
        else _display.DisplayError($"No items found on page {pageNumber}. Returning to page 1.");
    }
}
