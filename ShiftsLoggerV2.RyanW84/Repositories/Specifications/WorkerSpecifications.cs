using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;

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

        // Apply paging if specified
        if (filterOptions.PageNumber > 0 && filterOptions.PageSize > 0)
        {
            var skip = (filterOptions.PageNumber - 1) * filterOptions.PageSize;
            ApplyPaging(skip, filterOptions.PageSize);
        }
    }

    private Expression<Func<Worker, bool>>? BuildCriteria(WorkerFilterOptions filterOptions)
    {
        Expression<Func<Worker, bool>>? criteria = null;

        // Apply filters
        if (filterOptions.WorkerId.HasValue && filterOptions.WorkerId.Value > 0)
        {
            criteria = And(criteria, w => w.WorkerId == filterOptions.WorkerId.Value);
        }

        if (!string.IsNullOrEmpty(filterOptions.Name))
        {
            criteria = And(criteria, w => EF.Functions.Like(w.Name, $"%{filterOptions.Name}%"));
        }

        if (!string.IsNullOrEmpty(filterOptions.Email))
        {
            criteria = And(criteria, w =>
                w.Email != null && EF.Functions.Like(w.Email, $"%{filterOptions.Email}%"));
        }

        if (!string.IsNullOrEmpty(filterOptions.PhoneNumber))
        {
            criteria = And(criteria, w =>
                w.PhoneNumber != null && EF.Functions.Like(w.PhoneNumber, $"%{filterOptions.PhoneNumber}%"));
        }

        // Search implementation
        if (!string.IsNullOrWhiteSpace(filterOptions.Search))
        {
            criteria = And(criteria, w =>
                EF.Functions.Like(w.Name, $"%{filterOptions.Search}%")
                || (w.Email != null && EF.Functions.Like(w.Email, $"%{filterOptions.Search}%"))
                || (w.PhoneNumber != null && EF.Functions.Like(w.PhoneNumber, $"%{filterOptions.Search}%"))
                || w.WorkerId.ToString().Contains(filterOptions.Search));
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

    private Expression<Func<Worker, bool>> And(
        Expression<Func<Worker, bool>>? left,
        Expression<Func<Worker, bool>> right)
    {
        if (left == null)
            return right;

        var parameter = Expression.Parameter(typeof(Worker), "w");
        var leftBody = new ParameterReplacer(parameter).Visit(left.Body);
        var rightBody = new ParameterReplacer(parameter).Visit(right.Body);
        var andExpression = Expression.AndAlso(leftBody, rightBody);

        return Expression.Lambda<Func<Worker, bool>>(andExpression, parameter);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression _)
        {
            return _parameter;
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
