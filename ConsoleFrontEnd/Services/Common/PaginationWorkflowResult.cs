using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services.Common;

/// <summary>
/// Result of pagination workflow operations
/// </summary>
public class PaginationWorkflowResult
{
    public bool ShouldContinue { get; set; }
    public int NextPageNumber { get; set; }
    public bool ShouldSelectShift { get; set; }
    public PaginatedApiResponseDto<List<Shift>>? ShiftData { get; set; }
    public string? Message { get; set; }

    public static PaginationWorkflowResult Continue(int nextPage, PaginatedApiResponseDto<List<Shift>> data) =>
        new() { ShouldContinue = true, NextPageNumber = nextPage, ShiftData = data };

    public static PaginationWorkflowResult SelectShift(PaginatedApiResponseDto<List<Shift>> data) =>
        new() { ShouldContinue = true, ShouldSelectShift = true, ShiftData = data };

    public static PaginationWorkflowResult Exit() =>
        new() { ShouldContinue = false };

    public static PaginationWorkflowResult Failure(string message) =>
        new() { ShouldContinue = false, Message = message };
}
