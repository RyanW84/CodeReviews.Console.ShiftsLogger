using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Core.Repositories;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Repositories.Specifications;

namespace ShiftsLoggerV2.RyanW84.Repositories;

/// <summary>
/// Repository implementation for Shift entity operations
/// </summary>
public class ShiftRepository
    : BaseRepository<Shift, ShiftFilterOptions, ShiftApiRequestDto, ShiftApiRequestDto>,
        IShiftRepository
{
    public ShiftRepository(ShiftsLoggerDbContext dbContext)
        : base(dbContext) { }

    /// <summary>
    /// Detects whether a given time range overlaps any existing shift for the same worker or at the same location.
    /// </summary>
    public async Task<bool> HasOverlappingShiftAsync(
        int workerId,
        int locationId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int? excludeShiftId = null
    )
    {
        var spec = new OverlappingShiftSpecification(workerId, locationId, startTime, endTime, excludeShiftId);
        return await DbSet.ApplySpecification(spec).AnyAsync().ConfigureAwait(false);
    }

    protected override IQueryable<Shift> BuildQuery(ShiftFilterOptions filterOptions)
    {
        var spec = new ShiftSpecification(filterOptions);
        return DbSet.ApplySpecification(spec);
    }

    protected override async Task<Shift?> GetEntityByIdAsync(int id)
    {
        var spec = new ShiftByIdSpecification(id);
        return await DbSet.ApplySpecification(spec).FirstOrDefaultAsync();
    }

    protected override async Task<Shift> CreateEntityFromDtoAsync(ShiftApiRequestDto createDto)
    {
        // Validate that Worker and Location exist
        var workerExists = await DbContext.Workers.AnyAsync(w => w.WorkerId == createDto.WorkerId);
        if (!workerExists)
            throw new ArgumentException($"Worker with ID {createDto.WorkerId} does not exist.");

        var locationExists = await DbContext.Locations.AnyAsync(l =>
            l.LocationId == createDto.LocationId
        );
        if (!locationExists)
            throw new ArgumentException($"Location with ID {createDto.LocationId} does not exist.");

        if (createDto.StartTime >= createDto.EndTime)
            throw new ArgumentException("Start time must be before end time.");

        return new Shift
        {
            WorkerId = createDto.WorkerId,
            LocationId = createDto.LocationId,
            StartTime = createDto.StartTime,
            EndTime = createDto.EndTime,
        };
    }

    protected override async Task UpdateEntityFromDtoAsync(
        Shift entity,
        ShiftApiRequestDto updateDto
    )
    {
        // Validate that Worker and Location exist
        var workerExists = await DbContext.Workers.AnyAsync(w => w.WorkerId == updateDto.WorkerId);
        if (!workerExists)
            throw new ArgumentException($"Worker with ID {updateDto.WorkerId} does not exist.");

        var locationExists = await DbContext.Locations.AnyAsync(l =>
            l.LocationId == updateDto.LocationId
        );
        if (!locationExists)
            throw new ArgumentException($"Location with ID {updateDto.LocationId} does not exist.");

        if (updateDto.StartTime >= updateDto.EndTime)
            throw new ArgumentException("Start time must be before end time.");

        entity.WorkerId = updateDto.WorkerId;
        entity.LocationId = updateDto.LocationId;
        entity.StartTime = updateDto.StartTime;
        entity.EndTime = updateDto.EndTime;
    }
}
