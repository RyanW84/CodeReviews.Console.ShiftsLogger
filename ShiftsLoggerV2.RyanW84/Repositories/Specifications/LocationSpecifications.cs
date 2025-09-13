using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;

namespace ShiftsLoggerV2.RyanW84.Repositories.Specifications;

/// <summary>
/// Specification for Location queries with filtering, sorting, and includes
/// </summary>
public class LocationSpecification : BaseSpecification<Location>
{
    public LocationSpecification(LocationFilterOptions filterOptions)
    {
        // Add includes for related entities
        AddInclude(l => l.Shifts);

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

    private Expression<Func<Location, bool>>? BuildCriteria(LocationFilterOptions filterOptions)
    {
        Expression<Func<Location, bool>>? criteria = null;

        // Apply filters
        if (filterOptions.LocationId.HasValue && filterOptions.LocationId.Value > 0)
        {
            criteria = And(criteria, l => l.LocationId == filterOptions.LocationId.Value);
        }

        if (!string.IsNullOrEmpty(filterOptions.Name))
        {
            criteria = And(criteria, l => EF.Functions.Like(l.Name, $"%{filterOptions.Name}%"));
        }

        if (!string.IsNullOrEmpty(filterOptions.Town))
        {
            criteria = And(criteria, l => EF.Functions.Like(l.Town, $"%{filterOptions.Town}%"));
        }

        if (!string.IsNullOrEmpty(filterOptions.County))
        {
            criteria = And(criteria, l => EF.Functions.Like(l.County, $"%{filterOptions.County}%"));
        }

        if (!string.IsNullOrEmpty(filterOptions.Postcode))
        {
            criteria = And(criteria, l => EF.Functions.Like(l.Postcode, $"%{filterOptions.Postcode}%"));
        }

        if (!string.IsNullOrEmpty(filterOptions.Country))
        {
            criteria = And(criteria, l => EF.Functions.Like(l.Country, $"%{filterOptions.Country}%"));
        }

        // Search implementation
        if (!string.IsNullOrWhiteSpace(filterOptions.Search))
        {
            criteria = And(criteria, l =>
                EF.Functions.Like(l.Name, $"%{filterOptions.Search}%")
                || EF.Functions.Like(l.Address, $"%{filterOptions.Search}%")
                || EF.Functions.Like(l.Town, $"%{filterOptions.Search}%")
                || EF.Functions.Like(l.County, $"%{filterOptions.Search}%")
                || EF.Functions.Like(l.Postcode, $"%{filterOptions.Search}%")
                || EF.Functions.Like(l.Country, $"%{filterOptions.Search}%")
                || l.LocationId.ToString().Contains(filterOptions.Search));
        }

        return criteria;
    }

    private void ApplySorting(LocationFilterOptions filterOptions)
    {
        if (!string.IsNullOrWhiteSpace(filterOptions.SortBy))
        {
            var sortBy = filterOptions.SortBy.ToLowerInvariant();
            var sortOrder = filterOptions.SortOrder?.ToLowerInvariant() ?? "asc";

            switch (sortBy)
            {
                case "locationid":
                    if (sortOrder == "asc")
                        ApplyOrderBy(l => l.LocationId);
                    else
                        ApplyOrderByDescending(l => l.LocationId);
                    break;
                case "name":
                    if (sortOrder == "asc")
                        ApplyOrderBy(l => l.Name);
                    else
                        ApplyOrderByDescending(l => l.Name);
                    break;
                case "address":
                    if (sortOrder == "asc")
                        ApplyOrderBy(l => l.Address);
                    else
                        ApplyOrderByDescending(l => l.Address);
                    break;
                case "town":
                    if (sortOrder == "asc")
                        ApplyOrderBy(l => l.Town);
                    else
                        ApplyOrderByDescending(l => l.Town);
                    break;
                case "county":
                    if (sortOrder == "asc")
                        ApplyOrderBy(l => l.County);
                    else
                        ApplyOrderByDescending(l => l.County);
                    break;
                case "postcode":
                    if (sortOrder == "asc")
                        ApplyOrderBy(l => l.Postcode);
                    else
                        ApplyOrderByDescending(l => l.Postcode);
                    break;
                case "country":
                    if (sortOrder == "asc")
                        ApplyOrderBy(l => l.Country);
                    else
                        ApplyOrderByDescending(l => l.Country);
                    break;
                default:
                    ApplyOrderBy(l => l.Name); // Default sorting by name
                    break;
            }
        }
        else
        {
            ApplyOrderBy(l => l.Name); // Default sorting by name
        }
    }

    private Expression<Func<Location, bool>> And(
        Expression<Func<Location, bool>>? left,
        Expression<Func<Location, bool>> right)
    {
        if (left == null)
            return right;

        var parameter = Expression.Parameter(typeof(Location), "l");
        var leftBody = new ParameterReplacer(parameter).Visit(left.Body);
        var rightBody = new ParameterReplacer(parameter).Visit(right.Body);
        var andExpression = Expression.AndAlso(leftBody, rightBody);

        return Expression.Lambda<Func<Location, bool>>(andExpression, parameter);
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
/// Specification for getting a single location by ID with includes
/// </summary>
public class LocationByIdSpecification : BaseSpecification<Location>
{
    public LocationByIdSpecification(int locationId)
    {
        Criteria = l => l.LocationId == locationId;
        AddInclude(l => l.Shifts);
    }
}

/// <summary>
/// Specification for checking if location has associated shifts
/// </summary>
public class LocationHasShiftsSpecification : BaseSpecification<Location>
{
    public LocationHasShiftsSpecification(int locationId)
    {
        Criteria = l => l.LocationId == locationId;
        AddInclude(l => l.Shifts);
    }
}
