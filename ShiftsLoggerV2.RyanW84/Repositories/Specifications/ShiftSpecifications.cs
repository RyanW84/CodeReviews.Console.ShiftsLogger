using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Specifications.Common;

namespace ShiftsLoggerV2.RyanW84.Repositories.Specifications;

/// <summary>
/// Specification for Shift queries with filtering, sorting, and includes
/// </summary>
public class ShiftSpecification : BaseSpecification<Shift>
{
    public ShiftSpecification(ShiftFilterOptions filterOptions)
    {
        // Add includes for related entities
        AddInclude(s => s.Location!);
        AddInclude(s => s.Worker!);

        // Build criteria based on filter options
        var criteria = BuildCriteria(filterOptions);
        if (criteria != null)
        {
            Criteria = criteria;
        }

        // Apply sorting
        ApplySorting(filterOptions);

        // Note: Paging is handled by BaseRepository, not here
    }

    private Expression<Func<Shift, bool>>? BuildCriteria(ShiftFilterOptions filterOptions)
    {
        Expression<Func<Shift, bool>>? criteria = null;

        // Apply filters
        if (filterOptions.ShiftId is not 0)
        {
            criteria = ExpressionCombiner.And(criteria, s => s.ShiftId == filterOptions.ShiftId, "s");
        }

        if (filterOptions.WorkerId is not null and not 0)
        {
            criteria = ExpressionCombiner.And(criteria, s => s.WorkerId == filterOptions.WorkerId, "s");
        }

        if (filterOptions.LocationId is not null and not 0)
        {
            criteria = ExpressionCombiner.And(criteria, s => s.LocationId == filterOptions.LocationId, "s");
        }

        if (!string.IsNullOrEmpty(filterOptions.LocationName))
        {
            criteria = ExpressionCombiner.And(criteria, s =>
                s.Location != null
                && EF.Functions.Like(s.Location.Name, $"%{filterOptions.LocationName}%"), "s");
        }

        // Date filters
        if (filterOptions.StartTime is not null)
        {
            criteria = ExpressionCombiner.And(criteria, s => s.StartTime.Date >= filterOptions.StartTime.Value.Date, "s");
        }

        if (filterOptions.EndTime is not null)
        {
            criteria = ExpressionCombiner.And(criteria, s => s.EndTime.Date <= filterOptions.EndTime.Value.Date, "s");
        }

        // Duration filters
        if (filterOptions.MinDurationMinutes is not null and > 0)
        {
            criteria = ExpressionCombiner.And(criteria, s =>
                EF.Functions.DateDiffMinute(s.StartTime, s.EndTime)
                >= filterOptions.MinDurationMinutes, "s");
        }

        if (filterOptions.MaxDurationMinutes is not null and > 0)
        {
            criteria = ExpressionCombiner.And(criteria, s =>
                EF.Functions.DateDiffMinute(s.StartTime, s.EndTime)
                <= filterOptions.MaxDurationMinutes, "s");
        }

        // Search implementation
        if (!string.IsNullOrWhiteSpace(filterOptions.Search))
        {
            criteria = ExpressionCombiner.And(criteria, s =>
                s.WorkerId.ToString().Contains(filterOptions.Search)
                || s.LocationId.ToString().Contains(filterOptions.Search)
                || (s.Location != null && EF.Functions.Like(s.Location.Name, $"%{filterOptions.Search}%"))
                || (s.Location != null && EF.Functions.Like(s.Location.Town, $"%{filterOptions.Search}%"))
                || (s.Location != null && EF.Functions.Like(s.Location.Country, $"%{filterOptions.Search}%"))
                || s.StartTime.ToString().Contains(filterOptions.Search)
                || s.EndTime.ToString().Contains(filterOptions.Search), "s");
        }

        return criteria;
    }

    private void ApplySorting(ShiftFilterOptions filterOptions)
    {
        if (!string.IsNullOrWhiteSpace(filterOptions.SortBy))
        {
            var sortBy = filterOptions.SortBy.ToLowerInvariant();
            var sortOrder = filterOptions.SortOrder?.ToLowerInvariant() ?? "asc";

            switch (sortBy)
            {
                case "shiftid":
                    if (sortOrder == "asc")
                        ApplyOrderBy(s => s.ShiftId);
                    else
                        ApplyOrderByDescending(s => s.ShiftId);
                    break;
                case "starttime":
                    if (sortOrder == "asc")
                        ApplyOrderBy(s => s.StartTime);
                    else
                        ApplyOrderByDescending(s => s.StartTime);
                    break;
                case "endtime":
                    if (sortOrder == "asc")
                        ApplyOrderBy(s => s.EndTime);
                    else
                        ApplyOrderByDescending(s => s.EndTime);
                    break;
                case "workerid":
                    if (sortOrder == "asc")
                        ApplyOrderBy(s => s.WorkerId);
                    else
                        ApplyOrderByDescending(s => s.WorkerId);
                    break;
                case "locationid":
                    if (sortOrder == "asc")
                        ApplyOrderBy(s => s.LocationId);
                    else
                        ApplyOrderByDescending(s => s.LocationId);
                    break;
                case "locationname":
                    if (sortOrder == "asc")
                        ApplyOrderBy(s => s.Location != null ? s.Location.Name : "");
                    else
                        ApplyOrderByDescending(s => s.Location != null ? s.Location.Name : "");
                    break;
                case "duration":
                    if (sortOrder == "asc")
                        ApplyOrderBy(s => EF.Functions.DateDiffMinute(s.StartTime, s.EndTime));
                    else
                        ApplyOrderByDescending(s => EF.Functions.DateDiffMinute(s.StartTime, s.EndTime));
                    break;
                default:
                    ApplyOrderBy(s => s.ShiftId); // Default sorting
                    break;
            }
        }
        else
        {
            ApplyOrderBy(s => s.ShiftId); // Default sorting
        }
    }
}

/// <summary>
/// Specification for getting a single shift by ID with includes
/// </summary>
public class ShiftByIdSpecification : BaseSpecification<Shift>
{
    public ShiftByIdSpecification(int shiftId)
    {
        Criteria = s => s.ShiftId == shiftId;
        AddInclude(s => s.Location!);
        AddInclude(s => s.Worker!);
    }
}

/// <summary>
/// Specification for checking overlapping shifts
/// </summary>
public class OverlappingShiftSpecification : BaseSpecification<Shift>
{
    public OverlappingShiftSpecification(
        int workerId,
        int locationId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int? excludeShiftId = null)
    {
        var criteria = (Expression<Func<Shift, bool>>)(s =>
            (s.WorkerId == workerId || s.LocationId == locationId)
            && s.StartTime < endTime
            && startTime < s.EndTime);

        if (excludeShiftId.HasValue)
        {
            criteria = ExpressionCombiner.And(criteria, s => s.ShiftId != excludeShiftId.Value, "s");
        }

        Criteria = criteria;
    }
}
