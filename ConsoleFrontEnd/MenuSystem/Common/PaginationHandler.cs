using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
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
                var selected = await _input.GetMenuChoiceAsync($"Select Item (Page {response.PageNumber} of {response.TotalPages}):", choices.ToArray());

                var result = await handleSelectionFunc!(selected, response, currentPage, pageSize);
                if (result != null) return result;
                // If result is null, continue pagination
            }
            else
            {
                var choice = await GetPaginationChoiceAsync(response);
                var (shouldContinue, newPage, newSize, result) = await HandleChoiceAsync(choice, currentPage, pageSize, response);
                if (result != null) return result;
                if (!shouldContinue) return null;
                currentPage = newPage;
                pageSize = newSize;
            }
        }
    }

    private async Task<string> GetPaginationChoiceAsync<T>(ApiResponseDto<List<T>> response)
    {
        var options = new List<string>();
        if (response.HasPreviousPage) options.Add("Previous Page");
        if (response.HasNextPage) options.Add("Next Page");
        options.AddRange(new[] { "Go to Page", "Change Page Size", "Enter ID Manually", "Back to Menu" });
        return await _input.GetMenuChoiceAsync("Choose an action:", options.ToArray());
    }

    private async Task<(bool, int, int, object?)> HandleChoiceAsync<T>(string choice, int currentPage, int pageSize, ApiResponseDto<List<T>> response)
    {
        switch (choice)
        {
            case "Previous Page": return (true, currentPage - 1, pageSize, null);
            case "Next Page": return (true, currentPage + 1, pageSize, null);
            case "Go to Page": return (true, await HandleGoToPageAsync(response), pageSize, null);
            case "Change Page Size": return (true, 1, await HandleChangePageSizeAsync(pageSize), null);
            case "Enter ID Manually": return (false, currentPage, pageSize, await _input.GetIntegerInputAsync("[green]Enter ID:[/]") as object);
            default: return (false, currentPage, pageSize, null);
        }
    }

    private async Task<int> HandleGoToPageAsync<T>(ApiResponseDto<List<T>> response)
    {
        var page = await _input.GetIntegerInputAsync($"Enter page number (1-{response.TotalPages}):", 1, response.TotalPages);
        return page >= 1 && page <= response.TotalPages ? page : response.PageNumber;
    }

    private async Task<int> HandleChangePageSizeAsync(int currentSize)
    {
        var size = await _input.GetIntegerInputAsync("Enter new page size (1-1000):", 1, 1000);
        return size >= 1 && size <= 1000 ? size : currentSize;
    }

    private void HandleEmptyPage<T>(ApiResponseDto<List<T>> response, int pageNumber)
    {
        if (pageNumber == 1) _display.DisplayError("No items found.");
        else _display.DisplayError($"No items found on page {pageNumber}. Returning to page 1.");
    }
}
