using ShiftsLoggerV2.RyanW84.Core.Interfaces;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Common;

namespace ShiftsLoggerV2.RyanW84.Services.Interfaces;

/// <summary>
/// Business logic service interface for Shift operations
/// </summary>
public interface IShiftBusinessService
{
    Task<Result<List<Shift>>> GetAllAsync(ShiftFilterOptions filterOptions);
    Task<Result<Shift>> GetByIdAsync(int id);
    Task<Result<Shift>> CreateAsync(ShiftApiRequestDto createDto);
    Task<Result<Shift>> UpdateAsync(int id, ShiftApiRequestDto updateDto);
    Task<Result> DeleteAsync(int id);
}

/// <summary>
/// Business logic service interface for Worker operations
/// </summary>
public interface IWorkerBusinessService
{
    Task<Result<List<Worker>>> GetAllAsync(WorkerFilterOptions filterOptions);
    Task<Result<Worker>> GetByIdAsync(int id);
    Task<Result<Worker>> CreateAsync(WorkerApiRequestDto createDto);
    Task<Result<Worker>> UpdateAsync(int id, WorkerApiRequestDto updateDto);
    Task<Result> DeleteAsync(int id);
}

/// <summary>
/// Business logic service interface for Location operations
/// </summary>
public interface ILocationBusinessService
{
    Task<Result<List<Location>>> GetAllAsync(LocationFilterOptions filterOptions);
    Task<Result<Location>> GetByIdAsync(int id);
    Task<Result<Location>> CreateAsync(LocationApiRequestDto createDto);
    Task<Result<Location>> UpdateAsync(int id, LocationApiRequestDto updateDto);
    Task<Result> DeleteAsync(int id);
}
