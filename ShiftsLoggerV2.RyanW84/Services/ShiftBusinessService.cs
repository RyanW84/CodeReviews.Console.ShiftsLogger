using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services.Base;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Services;

/// <summary>
/// Business logic service for Shift operations
/// </summary>
public class ShiftBusinessService : BaseService<Shift, ShiftFilterOptions, ShiftApiRequestDto, ShiftApiRequestDto>, IShiftBusinessService
{
    private readonly IShiftRepository _shiftRepository;

    public ShiftBusinessService(IShiftRepository shiftRepository) : base(shiftRepository)
    {
        _shiftRepository = shiftRepository;
    }

    protected override async ValueTask<Result> ValidateForCreateAsync(ShiftApiRequestDto createDto)
    {
        // Business logic validation for shift creation
        // Basic required fields
        if (createDto.WorkerId <= 0)
            return Result.Failure("WorkerId must be greater than zero.");
        if (createDto.LocationId <= 0)
            return Result.Failure("LocationId must be greater than zero.");

        // Start must be before End
        if (createDto.StartTime >= createDto.EndTime)
            return Result.Failure("Start time must be before end time.");

        // Allowed date range: +/- 5 years from now (more forgiving)
        if (createDto.StartTime < DateTimeOffset.Now.AddYears(-5) || createDto.StartTime > DateTimeOffset.Now.AddYears(5))
            return Result.Failure("Start time is out of allowed range (5 years past/future)." );
        if (createDto.EndTime < DateTimeOffset.Now.AddYears(-5) || createDto.EndTime > DateTimeOffset.Now.AddYears(5))
            return Result.Failure("End time is out of allowed range (5 years past/future)." );

        // More forgiving past-start tolerance: 30 minutes instead of 5
        if (createDto.StartTime < DateTimeOffset.Now.AddMinutes(-30))
            return Result.Failure("Shift cannot start more than 30 minutes in the past.");

        var shiftDuration = createDto.EndTime - createDto.StartTime;
        if (shiftDuration.TotalMinutes < 5)
            return Result.Failure("Shift duration must be at least 5 minutes.");
        if (shiftDuration.TotalHours > 24)
            return Result.Failure("Shift duration cannot exceed 24 hours.");

        // Prevent overlapping shifts for same worker or location
        var hasOverlap = await _shiftRepository.HasOverlappingShiftAsync(createDto.WorkerId, createDto.LocationId, createDto.StartTime, createDto.EndTime).ConfigureAwait(false);
        if (hasOverlap)
            return Result.Failure("The requested shift overlaps an existing shift for the same worker or location.");

        return Result.Success();
    }

    protected override async ValueTask<Result> ValidateForUpdateAsync(int id, ShiftApiRequestDto updateDto)
    {
        // Business logic validation for shift updates
        var createValidation = await ValidateForCreateAsync(updateDto).ConfigureAwait(false);
        if (createValidation.IsFailure)
            return createValidation;

        // Additional update-specific validations could go here
        // Prevent overlapping with other shifts (exclude the current shift id)
        var hasOverlap = await _shiftRepository.HasOverlappingShiftAsync(updateDto.WorkerId, updateDto.LocationId, updateDto.StartTime, updateDto.EndTime, id).ConfigureAwait(false);
        if (hasOverlap)
            return Result.Failure("The requested update would cause an overlap with an existing shift for the same worker or location.");
        return Result.Success();
    }

    protected override async Task<Result> ValidateForDeleteAsync(int id)
    {
        // Business logic validation for shift deletion
        var shiftResult = await _shiftRepository.GetByIdAsync(id).ConfigureAwait(false);
        if (shiftResult.IsFailure)
            return shiftResult;

        var shift = shiftResult.Data!;

        // More forgiving: only prevent deletion of shifts that started more than 1 hour ago
        if (shift.StartTime <= DateTimeOffset.Now.AddHours(-1))
            return Result.Failure("Cannot delete shifts that started more than 1 hour ago.");

        return Result.Success();
    }
}
