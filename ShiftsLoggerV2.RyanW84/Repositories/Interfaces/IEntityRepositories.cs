using ShiftsLoggerV2.RyanW84.Core.Interfaces;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;

namespace ShiftsLoggerV2.RyanW84.Repositories.Interfaces;

/// <summary>
/// Repository interface for Shift operations
/// </summary>
public interface IShiftRepository : IRepository<Shift, ShiftFilterOptions, ShiftApiRequestDto, ShiftApiRequestDto>
{
	/// <summary>
	/// Check whether the specified time range overlaps any existing shift for the same worker or at the same location.
	/// If excludeShiftId is provided, that shift will be ignored (useful for updates).
	/// </summary>
	Task<bool> HasOverlappingShiftAsync(int workerId, int locationId, DateTimeOffset startTime, DateTimeOffset endTime, int? excludeShiftId = null);
}

/// <summary>
/// Repository interface for Worker operations
/// </summary>
public interface IWorkerRepository : IRepository<Worker, WorkerFilterOptions, WorkerApiRequestDto, WorkerApiRequestDto>
{
    /// <summary>
    /// Checks if a worker has any associated shifts
    /// </summary>
    /// <param name="workerId">The worker ID to check</param>
    /// <returns>True if the worker has associated shifts, false otherwise</returns>
    Task<bool> HasAssociatedShiftsAsync(int workerId);
}

/// <summary>
/// Repository interface for Location operations
/// </summary>
public interface ILocationRepository : IRepository<Location, LocationFilterOptions, LocationApiRequestDto, LocationApiRequestDto>
{
    /// <summary>
    /// Checks if a location has any associated shifts
    /// </summary>
    /// <param name="locationId">The location ID to check</param>
    /// <returns>True if the location has associated shifts, false otherwise</returns>
    Task<bool> HasAssociatedShiftsAsync(int locationId);
}
