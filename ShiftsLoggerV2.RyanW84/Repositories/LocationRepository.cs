using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Core.Repositories;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Repositories;

/// <summary>
/// Repository implementation for Location entity operations
/// </summary>
public class LocationRepository : BaseRepository<Location, LocationFilterOptions, LocationApiRequestDto, LocationApiRequestDto>, ILocationRepository
{
    public LocationRepository(ShiftsLoggerDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<Location> BuildQuery(LocationFilterOptions filterOptions)
    {
        var query = DbSet.AsQueryable();

        // Include Shifts navigation property for count display
        query = query.Include(l => l.Shifts);

        // Apply filters
        if (filterOptions.LocationId.HasValue && filterOptions.LocationId.Value > 0)
            query = query.Where(l => l.LocationId == filterOptions.LocationId.Value);

        if (!string.IsNullOrEmpty(filterOptions.Name))
            query = query.Where(l => EF.Functions.Like(l.Name, $"%{filterOptions.Name}%"));

        if (!string.IsNullOrEmpty(filterOptions.Town))
            query = query.Where(l => EF.Functions.Like(l.Town, $"%{filterOptions.Town}%"));

        if (!string.IsNullOrEmpty(filterOptions.County))
            query = query.Where(l => EF.Functions.Like(l.County, $"%{filterOptions.County}%"));

        if (!string.IsNullOrEmpty(filterOptions.PostCode))
            query = query.Where(l => EF.Functions.Like(l.PostCode, $"%{filterOptions.PostCode}%"));

        if (!string.IsNullOrEmpty(filterOptions.Country))
            query = query.Where(l => EF.Functions.Like(l.Country, $"%{filterOptions.Country}%"));

        // Search implementation
        if (!string.IsNullOrWhiteSpace(filterOptions.Search))
            query = query.Where(l =>
                EF.Functions.Like(l.Name, $"%{filterOptions.Search}%") ||
                EF.Functions.Like(l.Address, $"%{filterOptions.Search}%") ||
                EF.Functions.Like(l.Town, $"%{filterOptions.Search}%") ||
                EF.Functions.Like(l.County, $"%{filterOptions.Search}%") ||
                EF.Functions.Like(l.PostCode, $"%{filterOptions.Search}%") ||
                EF.Functions.Like(l.Country, $"%{filterOptions.Search}%") ||
                l.LocationId.ToString().Contains(filterOptions.Search));

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filterOptions.SortBy))
        {
            var sortBy = filterOptions.SortBy.ToLowerInvariant();
            var sortOrder = filterOptions.SortOrder?.ToLowerInvariant() ?? "asc";

            query = sortBy switch
            {
                "locationid" => sortOrder == "asc"
                    ? query.OrderBy(l => l.LocationId)
                    : query.OrderByDescending(l => l.LocationId),
                "name" => sortOrder == "asc"
                    ? query.OrderBy(l => l.Name)
                    : query.OrderByDescending(l => l.Name),
                "address" => sortOrder == "asc"
                    ? query.OrderBy(l => l.Address)
                    : query.OrderByDescending(l => l.Address),
                "town" => sortOrder == "asc"
                    ? query.OrderBy(l => l.Town)
                    : query.OrderByDescending(l => l.Town),
                "county" => sortOrder == "asc"
                    ? query.OrderBy(l => l.County)
                    : query.OrderByDescending(l => l.County),
                "postcode" => sortOrder == "asc"
                    ? query.OrderBy(l => l.PostCode)
                    : query.OrderByDescending(l => l.PostCode),
                "country" => sortOrder == "asc"
                    ? query.OrderBy(l => l.Country)
                    : query.OrderByDescending(l => l.Country),
                _ => sortOrder == "asc"
                    ? query.OrderBy(l => l.LocationId)
                    : query.OrderByDescending(l => l.LocationId)
            };
        }
        else
        {
            query = query.OrderBy(l => l.Name); // Default sorting by name
        }

        return query;
    }

    protected override async Task<Location?> GetEntityByIdAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(l => l.LocationId == id);
    }

    protected override async Task<Location> CreateEntityFromDtoAsync(LocationApiRequestDto createDto)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(createDto.Name))
            throw new ArgumentException("Location name is required.");

        if (string.IsNullOrWhiteSpace(createDto.Address))
            throw new ArgumentException("Location address is required.");

        if (string.IsNullOrWhiteSpace(createDto.Town))
            throw new ArgumentException("Location town is required.");

        if (string.IsNullOrWhiteSpace(createDto.County))
            throw new ArgumentException("Location county is required.");

        if (string.IsNullOrWhiteSpace(createDto.PostCode))
            throw new ArgumentException("Location post code is required.");

        if (string.IsNullOrWhiteSpace(createDto.Country))
            throw new ArgumentException("Location country is required.");

        // Check for duplicate location name
        var nameExists = await DbContext.Locations.AnyAsync(l => l.Name == createDto.Name.Trim());
        if (nameExists)
            throw new ArgumentException($"A location with name {createDto.Name} already exists.");

        return new Location
        {
            Name = createDto.Name.Trim(),
            Address = createDto.Address.Trim(),
            Town = createDto.Town.Trim(),
            County = createDto.County.Trim(),
            PostCode = createDto.PostCode.Trim(),
            Country = createDto.Country.Trim()
        };
    }

    protected override async Task UpdateEntityFromDtoAsync(Location entity, LocationApiRequestDto updateDto)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(updateDto.Name))
            throw new ArgumentException("Location name is required.");

        if (string.IsNullOrWhiteSpace(updateDto.Address))
            throw new ArgumentException("Location address is required.");

        if (string.IsNullOrWhiteSpace(updateDto.Town))
            throw new ArgumentException("Location town is required.");

        if (string.IsNullOrWhiteSpace(updateDto.County))
            throw new ArgumentException("Location county is required.");

        if (string.IsNullOrWhiteSpace(updateDto.PostCode))
            throw new ArgumentException("Location post code is required.");

        if (string.IsNullOrWhiteSpace(updateDto.Country))
            throw new ArgumentException("Location country is required.");

        // Check for duplicate location name if different from current
        if (updateDto.Name.Trim() != entity.Name)
        {
            var nameExists = await DbContext.Locations.AnyAsync(l => l.Name == updateDto.Name.Trim() && l.LocationId != entity.LocationId);
            if (nameExists)
                throw new ArgumentException($"A location with name {updateDto.Name} already exists.");
        }

        entity.Name = updateDto.Name.Trim();
        entity.Address = updateDto.Address.Trim();
        entity.Town = updateDto.Town.Trim();
        entity.County = updateDto.County.Trim();
        entity.PostCode = updateDto.PostCode.Trim();
        entity.Country = updateDto.Country.Trim();
    }

    /// <summary>
    /// Checks if a location has any associated shifts
    /// </summary>
    /// <param name="locationId">The location ID to check</param>
    /// <returns>True if the location has associated shifts, false otherwise</returns>
    public async Task<bool> HasAssociatedShiftsAsync(int locationId)
    {
        return await DbContext.Shifts.AnyAsync(s => s.LocationId == locationId);
    }
}
