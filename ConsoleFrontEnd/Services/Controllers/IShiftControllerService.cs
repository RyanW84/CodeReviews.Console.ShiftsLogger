using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Business;
using ConsoleFrontEnd.Services.Common;

namespace ConsoleFrontEnd.Services.Controllers;

/// <summary>
/// Console-friendly controller interface that abstracts HTTP concerns
/// Provides the same business logic as web controllers but without HTTP dependencies
/// </summary>
public interface IShiftControllerService
{
    /// <summary>
    /// Gets all shifts with pagination and filtering
    /// </summary>
    Task<OperationResult<PaginatedApiResponseDto<List<Shift>>>> GetAllShiftsAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Gets a shift by ID
    /// </summary>
    Task<OperationResult<Shift>> GetShiftByIdAsync(int id);

    /// <summary>
    /// Creates a new shift
    /// </summary>
    Task<OperationResult<Shift>> CreateShiftAsync(ShiftApiRequestDto shiftDto);

    /// <summary>
    /// Updates an existing shift
    /// </summary>
    Task<OperationResult<Shift>> UpdateShiftAsync(int id, ShiftApiRequestDto shiftDto);

    /// <summary>
    /// Deletes a shift
    /// </summary>
    Task<OperationResult> DeleteShiftAsync(int id);

    /// <summary>
    /// Filters shifts based on criteria
    /// </summary>
    Task<OperationResult<List<Shift>>> FilterShiftsAsync(ShiftFilterOptions options);
}
