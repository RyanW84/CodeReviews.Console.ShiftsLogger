using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Specifications.Common;

namespace ShiftsLoggerV2.RyanW84.Repositories.Specifications;

/// <summary>
/// Specification for Worker queries with filtering, sorting, and includes
/// </summary>
public class WorkerSpecification : BaseSpecification<Worker>
{
    public WorkerSpecification(WorkerFilterOptions filterOptions)
    {
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

    private Expression<Func<Worker, bool>>? BuildCriteria(WorkerFilterOptions filterOptions)
    {
        Expression<Func<Worker, bool>>? criteria = null;

        // Apply filters
        if (filterOptions.WorkerId.HasValue && filterOptions.WorkerId.Value > 0)
        {
            criteria = ExpressionCombiner.And(criteria, w => w.WorkerId == filterOptions.WorkerId.Value, "w");
        }

        if (!string.IsNullOrEmpty(filterOptions.Name))
        {
            criteria = ExpressionCombiner.And(criteria, w => EF.Functions.Like(w.Name, $"%{filterOptions.Name}%"), "w");
        }

        if (!string.IsNullOrEmpty(filterOptions.Email))
        {
            criteria = ExpressionCombiner.And(criteria, w =>
                w.Email != null && EF.Functions.Like(w.Email, $"%{filterOptions.Email}%"), "w");
        }

        if (!string.IsNullOrEmpty(filterOptions.PhoneNumber))
        {
            criteria = ExpressionCombiner.And(criteria, w =>
                w.PhoneNumber != null && EF.Functions.Like(w.PhoneNumber, $"%{filterOptions.PhoneNumber}%"), "w");
        }

        // Search implementation
        if (!string.IsNullOrWhiteSpace(filterOptions.Search))
        {
            criteria = ExpressionCombiner.And(criteria, w =>
                EF.Functions.Like(w.Name, $"%{filterOptions.Search}%")
                || (w.Email != null && EF.Functions.Like(w.Email, $"%{filterOptions.Search}%"))
                || (w.PhoneNumber != null && EF.Functions.Like(w.PhoneNumber, $"%{filterOptions.Search}%"))
                || w.WorkerId.ToString().Contains(filterOptions.Search), "w");
        }

        return criteria;
    }

    private void ApplySorting(WorkerFilterOptions filterOptions)
    {
        if (!string.IsNullOrWhiteSpace(filterOptions.SortBy))
        {
            var sortBy = filterOptions.SortBy.ToLowerInvariant();
            var sortOrder = filterOptions.SortOrder?.ToLowerInvariant() ?? "asc";

            switch (sortBy)
            {
                case "workerid":
                    if (sortOrder == "asc")
                        ApplyOrderBy(w => w.WorkerId);
                    else
                        ApplyOrderByDescending(w => w.WorkerId);
                    break;
                case "name":
                    if (sortOrder == "asc")
                        ApplyOrderBy(w => w.Name);
                    else
                        ApplyOrderByDescending(w => w.Name);
                    break;
                case "email":
                    if (sortOrder == "asc")
                        ApplyOrderBy(w => w.Email ?? "");
                    else
                        ApplyOrderByDescending(w => w.Email ?? "");
                    break;
                case "phonenumber":
                    if (sortOrder == "asc")
                        ApplyOrderBy(w => w.PhoneNumber ?? "");
                    else
                        ApplyOrderByDescending(w => w.PhoneNumber ?? "");
                    break;
                default:
                    ApplyOrderBy(w => w.WorkerId); // Default sorting
                    break;
            }
        }
        else
        {
            ApplyOrderBy(w => w.WorkerId); // Default sorting
        }
    }
}

/// <summary>
/// Specification for getting a single worker by ID
/// </summary>
public class WorkerByIdSpecification : BaseSpecification<Worker>
{
    public WorkerByIdSpecification(int workerId)
    {
        Criteria = w => w.WorkerId == workerId;
    }
}

/// <summary>
/// Specification for checking if worker has associated shifts
/// </summary>
public class WorkerHasShiftsSpecification : BaseSpecification<Worker>
{
    public WorkerHasShiftsSpecification(int workerId)
    {
        Criteria = w => w.WorkerId == workerId;
        AddInclude(w => w.Shifts);
    }
}
