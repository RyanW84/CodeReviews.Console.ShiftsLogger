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
/// Business logic service for Worker operations
/// </summary>
public class WorkerBusinessService : BaseService<Worker, WorkerFilterOptions, WorkerApiRequestDto, WorkerApiRequestDto>, IWorkerBusinessService
{
    private readonly IWorkerRepository _workerRepository;

    public WorkerBusinessService(IWorkerRepository workerRepository) : base(workerRepository)
    {
        _workerRepository = workerRepository;
    }

    protected override ValueTask<Result> ValidateForCreateAsync(WorkerApiRequestDto createDto)
    {
        // Business logic validation for worker creation
        if (string.IsNullOrWhiteSpace(createDto.Name))
            return ValueTask.FromResult(Result.Failure("Worker name is required."));

        if (createDto.Name.Length < 1)
            return ValueTask.FromResult(Result.Failure("Worker name must be at least 1 character long."));

        if (createDto.Name.Length > 100)
            return ValueTask.FromResult(Result.Failure("Worker name cannot exceed 100 characters."));

        // Email validation - more forgiving
        if (!string.IsNullOrWhiteSpace(createDto.Email))
        {
            if (!IsValidEmail(createDto.Email))
                return ValueTask.FromResult(Result.Failure("Email must be in basic format: user@domain.extension"));

            if (createDto.Email.Length > 254)
                return ValueTask.FromResult(Result.Failure("Email address cannot exceed 254 characters."));
        }

        // Phone number validation - more forgiving
        if (!string.IsNullOrWhiteSpace(createDto.PhoneNumber))
        {
            if (!IsValidPhoneNumber(createDto.PhoneNumber))
                return ValueTask.FromResult(Result.Failure("Phone number must contain at least 10 digits."));

            if (createDto.PhoneNumber.Length > 20)
                return ValueTask.FromResult(Result.Failure("Phone number cannot exceed 20 characters."));
        }

        // At least one contact method required
        if (string.IsNullOrWhiteSpace(createDto.Email) && string.IsNullOrWhiteSpace(createDto.PhoneNumber))
            return ValueTask.FromResult(Result.Failure("At least one contact method (email or phone) is required."));

        return ValueTask.FromResult(Result.Success());
    }

    protected override async ValueTask<Result> ValidateForUpdateAsync(int id, WorkerApiRequestDto updateDto)
    {
        // Business logic validation for worker updates
        var createValidation = await ValidateForCreateAsync(updateDto).ConfigureAwait(false);
        if (createValidation.IsFailure)
            return createValidation;

        // Check if worker has active shifts before major updates
        var workerResult = await _workerRepository.GetByIdAsync(id).ConfigureAwait(false);
        if (workerResult.IsFailure)
            return workerResult;

        // Additional update-specific validations could go here
        return Result.Success();
    }

    protected override async Task<Result> ValidateForDeleteAsync(int id)
    {
        // Business logic validation for worker deletion
        var workerResult = await _workerRepository.GetByIdAsync(id).ConfigureAwait(false);
        if (workerResult.IsFailure)
            return workerResult;

        var worker = workerResult.Data!;
        
        // Check if worker has any associated shifts
        var hasShifts = await _workerRepository.HasAssociatedShiftsAsync(id).ConfigureAwait(false);
            
        if (hasShifts)
        {
            return Result.Failure($"Cannot delete worker '{worker.Name}' because they have associated shifts. Please reassign or delete the shifts first.");
        }

        return Result.Success();
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Use a simple regex pattern for email validation
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove formatting characters for validation
        var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(".", "");

        // Must contain at least 10 digits
        var digitCount = cleanPhone.Count(char.IsDigit);
        return digitCount >= 10 && digitCount <= 15;
    }
}
