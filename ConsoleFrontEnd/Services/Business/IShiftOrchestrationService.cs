using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.Services.Business;

/// <summary>
/// Orchestrates complex shift operations involving multiple services
/// Follows Single Responsibility Principle by handling business workflows
/// </summary>
public interface IShiftOrchestrationService
{
    /// <summary>
    /// Orchestrates the complete shift creation workflow
    /// </summary>
    Task<OperationResult<Shift>> CreateShiftWorkflowAsync();

    /// <summary>
    /// Orchestrates the complete shift update workflow
    /// </summary>
    Task<OperationResult<Shift>> UpdateShiftWorkflowAsync(int shiftId);

    /// <summary>
    /// Orchestrates the complete shift deletion workflow
    /// </summary>
    Task<OperationResult> DeleteShiftWorkflowAsync(int shiftId);

    /// <summary>
    /// Orchestrates filtering shifts with user-friendly input
    /// </summary>
    Task<OperationResult<List<Shift>>> FilterShiftsWorkflowAsync();

    /// <summary>
    /// Gets shifts by worker with proper error handling
    /// </summary>
    Task<OperationResult<List<Shift>>> GetShiftsByWorkerWorkflowAsync(int workerId);

    /// <summary>
    /// Gets shifts by date range with validation
    /// </summary>
    Task<OperationResult<List<Shift>>> GetShiftsByDateRangeWorkflowAsync(DateTime startDate, DateTime endDate);
}
