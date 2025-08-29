using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Core.Repositories;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Repositories;

/// <summary>
/// Repository implementation for Shift entity operations
/// </summary>
public class ShiftRepository : BaseRepository<Shift, ShiftFilterOptions, ShiftApiRequestDto, ShiftApiRequestDto>, IShiftRepository
{
    public ShiftRepository(ShiftsLoggerDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Detects whether a given time range overlaps any existing shift for the same worker or at the same location.
    /// </summary>
    public async Task<bool> HasOverlappingShiftAsync(int workerId, int locationId, DateTimeOffset startTime, DateTimeOffset endTime, int? excludeShiftId = null)
    {
        // Two intervals [A,B) and [C,D) overlap when A < D && C < B
        var query = DbSet.AsQueryable();

        if (excludeShiftId is not null)
            query = query.Where(s => s.ShiftId != excludeShiftId.Value);

        // Check either same worker overlapping OR same location overlapping
        return await query.AnyAsync(s =>
            (s.WorkerId == workerId || s.LocationId == locationId) &&
            s.StartTime < endTime && startTime < s.EndTime
        ).ConfigureAwait(false);
    }

    protected override IQueryable<Shift> BuildQuery(ShiftFilterOptions filterOptions)
    {
        var query = DbSet
            .Include(s => s.Location)
            .Include(s => s.Worker)
            .AsQueryable();

        // Apply filters
        if (filterOptions.ShiftId is not 0)
            query = query.Where(s => s.ShiftId == filterOptions.ShiftId);

        if (filterOptions.WorkerId is not null and not 0)
            query = query.Where(s => s.WorkerId == filterOptions.WorkerId);

        if (filterOptions.LocationId is not null and not 0)
            query = query.Where(s => s.LocationId == filterOptions.LocationId);

        if (!string.IsNullOrEmpty(filterOptions.LocationName))
            query = query.Where(s =>
                s.Location != null && EF.Functions.Like(s.Location.Name, $"%{filterOptions.LocationName}%"));

        // Date filters
        if (filterOptions.StartTime is not null)
            query = query.Where(s => s.StartTime.Date >= filterOptions.StartTime.Value.Date);

        if (filterOptions.EndTime is not null)
            query = query.Where(s => s.EndTime.Date <= filterOptions.EndTime.Value.Date);

        // Duration filters
        if (filterOptions.MinDurationMinutes is not null and > 0)
            query = query.Where(s => EF.Functions.DateDiffMinute(s.StartTime, s.EndTime) >= filterOptions.MinDurationMinutes);

        if (filterOptions.MaxDurationMinutes is not null and > 0)
            query = query.Where(s => EF.Functions.DateDiffMinute(s.StartTime, s.EndTime) <= filterOptions.MaxDurationMinutes);

        // Search implementation
        if (!string.IsNullOrWhiteSpace(filterOptions.Search))
            query = query.Where(s =>
                s.WorkerId.ToString().Contains(filterOptions.Search) ||
                s.LocationId.ToString().Contains(filterOptions.Search) ||
                (s.Location != null && EF.Functions.Like(s.Location.Name, $"%{filterOptions.Search}%")) ||
                (s.Location != null && EF.Functions.Like(s.Location.Town, $"%{filterOptions.Search}%")) ||
                (s.Location != null && EF.Functions.Like(s.Location.Country, $"%{filterOptions.Search}%")) ||
                s.StartTime.ToString().Contains(filterOptions.Search) ||
                s.EndTime.ToString().Contains(filterOptions.Search));

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filterOptions.SortBy))
        {
            var sortBy = filterOptions.SortBy.ToLowerInvariant();
            var sortOrder = filterOptions.SortOrder?.ToLowerInvariant() ?? "asc";

            query = sortBy switch
            {
                "shiftid" => sortOrder == "asc"
                    ? query.OrderBy(s => s.ShiftId)
                    : query.OrderByDescending(s => s.ShiftId),
                "starttime" => sortOrder == "asc"
                    ? query.OrderBy(s => s.StartTime)
                    : query.OrderByDescending(s => s.StartTime),
                "endtime" => sortOrder == "asc"
                    ? query.OrderBy(s => s.EndTime)
                    : query.OrderByDescending(s => s.EndTime),
                "workerid" => sortOrder == "asc"
                    ? query.OrderBy(s => s.WorkerId)
                    : query.OrderByDescending(s => s.WorkerId),
                "locationid" => sortOrder == "asc"
                    ? query.OrderBy(s => s.LocationId)
                    : query.OrderByDescending(s => s.LocationId),
                "locationname" => sortOrder == "asc"
                    ? query.OrderBy(s => s.Location != null ? s.Location.Name : "")
                    : query.OrderByDescending(s => s.Location != null ? s.Location.Name : ""),
                "duration" => sortOrder == "asc"
                    ? query.OrderBy(s => EF.Functions.DateDiffMinute(s.StartTime, s.EndTime))
                    : query.OrderByDescending(s => EF.Functions.DateDiffMinute(s.StartTime, s.EndTime)),
                _ => sortOrder == "asc"
                    ? query.OrderBy(s => s.ShiftId)
                    : query.OrderByDescending(s => s.ShiftId)
            };
        }
        else
        {
            query = query.OrderBy(s => s.ShiftId); // Default sorting
        }

        return query;
    }

    protected override async Task<Shift?> GetEntityByIdAsync(int id)
    {
        return await DbSet
            .Include(s => s.Location)
            .Include(s => s.Worker)
            .FirstOrDefaultAsync(s => s.ShiftId == id);
    }

    protected override async Task<Shift> CreateEntityFromDtoAsync(ShiftApiRequestDto createDto)
    {
        // Validate that Worker and Location exist
        var workerExists = await DbContext.Workers.AnyAsync(w => w.WorkerId == createDto.WorkerId);
        if (!workerExists)
            throw new ArgumentException($"Worker with ID {createDto.WorkerId} does not exist.");

        var locationExists = await DbContext.Locations.AnyAsync(l => l.LocationId == createDto.LocationId);
        if (!locationExists)
            throw new ArgumentException($"Location with ID {createDto.LocationId} does not exist.");

        if (createDto.StartTime >= createDto.EndTime)
            throw new ArgumentException("Start time must be before end time.");

        return new Shift
        {
            WorkerId = createDto.WorkerId,
            LocationId = createDto.LocationId,
            StartTime = createDto.StartTime,
            EndTime = createDto.EndTime
        };
    }

    protected override async Task UpdateEntityFromDtoAsync(Shift entity, ShiftApiRequestDto updateDto)
    {
        // Validate that Worker and Location exist
        var workerExists = await DbContext.Workers.AnyAsync(w => w.WorkerId == updateDto.WorkerId);
        if (!workerExists)
            throw new ArgumentException($"Worker with ID {updateDto.WorkerId} does not exist.");

        var locationExists = await DbContext.Locations.AnyAsync(l => l.LocationId == updateDto.LocationId);
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
