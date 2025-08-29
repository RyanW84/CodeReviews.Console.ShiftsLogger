using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services.Base;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;
using System.Text.RegularExpressions;

namespace ShiftsLoggerV2.RyanW84.Services;

/// <summary>
/// Business logic service for Location operations
/// </summary>
public class LocationBusinessService : BaseService<Location, LocationFilterOptions, LocationApiRequestDto, LocationApiRequestDto>, ILocationBusinessService
{
    private readonly ILocationRepository _locationRepository;

    public LocationBusinessService(ILocationRepository locationRepository) : base(locationRepository)
    {
        _locationRepository = locationRepository;
    }

    protected override ValueTask<Result> ValidateForCreateAsync(LocationApiRequestDto createDto)
    {
        // Business logic validation for location creation
        var validationResult = ValidateLocationData(createDto);
        if (validationResult.IsFailure)
            return ValueTask.FromResult(validationResult);

        return ValueTask.FromResult(Result.Success());
    }

    protected override async ValueTask<Result> ValidateForUpdateAsync(int id, LocationApiRequestDto updateDto)
    {
        // Business logic validation for location updates
        var createValidation = await ValidateForCreateAsync(updateDto).ConfigureAwait(false);
        if (createValidation.IsFailure)
            return createValidation;

        // Check if location has active shifts before major updates
        var locationResult = await _locationRepository.GetByIdAsync(id).ConfigureAwait(false);
        if (locationResult.IsFailure)
            return locationResult;

        // Additional update-specific validations could go here
        return Result.Success();
    }

    protected override async Task<Result> ValidateForDeleteAsync(int id)
    {
        // Business logic validation for location deletion
        var locationResult = await _locationRepository.GetByIdAsync(id).ConfigureAwait(false);
        if (locationResult.IsFailure)
            return locationResult;

        var location = locationResult.Data!;
        
        // Check if location has any associated shifts
        var hasShifts = await _locationRepository.HasAssociatedShiftsAsync(id).ConfigureAwait(false);
            
        if (hasShifts)
        {
            return Result.Failure($"Cannot delete location '{location.Name}' because it has associated shifts. Please reassign or delete the shifts first.");
        }

        return Result.Success();
    }

    private static Result ValidateLocationData(LocationApiRequestDto dto)
    {
        // Name validation - more forgiving
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result.Failure("Location name is required.");

        if (dto.Name.Length < 1)
            return Result.Failure("Location name must be at least 1 character long.");

        if (dto.Name.Length > 100)
            return Result.Failure("Location name cannot exceed 100 characters.");

        // Address validation - more forgiving
        if (string.IsNullOrWhiteSpace(dto.Address))
            return Result.Failure("Location address is required.");

        if (dto.Address.Length < 3)
            return Result.Failure("Location address must be at least 3 characters long.");

        if (dto.Address.Length > 200)
            return Result.Failure("Location address cannot exceed 200 characters.");

        // Town validation
        if (string.IsNullOrWhiteSpace(dto.Town))
            return Result.Failure("Location town is required.");

        if (dto.Town.Length < 1)
            return Result.Failure("Location town must be at least 1 character long.");

        if (dto.Town.Length > 100)
            return Result.Failure("Location town cannot exceed 100 characters.");

        // County validation
        if (string.IsNullOrWhiteSpace(dto.County))
            return Result.Failure("Location county is required.");

        if (dto.County.Length < 1)
            return Result.Failure("Location county must be at least 1 character long.");

        if (dto.County.Length > 100)
            return Result.Failure("Location county cannot exceed 100 characters.");

        // Post code validation - more forgiving
        if (string.IsNullOrWhiteSpace(dto.PostCode))
            return Result.Failure("Location post code is required.");

        if (!IsValidPostCode(dto.PostCode))
            return Result.Failure("Post code must be at least 3 characters with letters or digits.");

        if (dto.PostCode.Length > 20)
            return Result.Failure("Location post code cannot exceed 20 characters.");

        // Country validation
        if (string.IsNullOrWhiteSpace(dto.Country))
            return Result.Failure("Location country is required.");

        if (dto.Country.Length < 2)
            return Result.Failure("Location country must be at least 2 characters long.");

        if (dto.Country.Length > 100)
            return Result.Failure("Location country cannot exceed 100 characters.");

        return Result.Success();
    }

    private static bool IsValidPostCode(string postCode)
    {
        if (string.IsNullOrWhiteSpace(postCode))
            return false;

        // Basic post code validation - allows alphanumeric characters, spaces, and dashes
        // This is a simplified pattern that works for most international formats
        var postCodePattern = @"^[A-Za-z0-9\s\-]+$";

        try
        {
            return Regex.IsMatch(postCode.Trim(), postCodePattern) &&
                   postCode.Trim().Length >= 3 &&
                   postCode.Any(char.IsLetterOrDigit);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
