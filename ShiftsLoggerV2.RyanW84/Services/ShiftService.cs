using System.Net;
using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Services;

public class ShiftService(IShiftRepository shiftRepository) : IShiftService
{
    private readonly IShiftRepository _shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));

    public async Task<ApiResponseDto<List<Shift?>>> GetAllShifts(ShiftFilterOptions shiftOptions)
    {
        var result = await _shiftRepository.GetAllAsync(shiftOptions).ConfigureAwait(false);
        return new ApiResponseDto<List<Shift?>>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.StatusCode,
            Message = result.Message,
            Data = result.Data?.Cast<Shift?>().ToList() ?? []
        };
    }

    public async Task<ApiResponseDto<Shift>> GetShiftById(int id)
    {
        var result = await _shiftRepository.GetByIdAsync(id).ConfigureAwait(false);
        return new ApiResponseDto<Shift>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.StatusCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<Shift>> CreateShift(ShiftApiRequestDto shift)
    {
        var result = await _shiftRepository.CreateAsync(shift).ConfigureAwait(false);
        return new ApiResponseDto<Shift>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.StatusCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<Shift>> UpdateShift(int id, ShiftApiRequestDto updatedShift)
    {
        var result = await _shiftRepository.UpdateAsync(id, updatedShift).ConfigureAwait(false);
        return new ApiResponseDto<Shift>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.StatusCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<string>> DeleteShift(int id)
    {
        var result = await _shiftRepository.DeleteAsync(id).ConfigureAwait(false);
        return new ApiResponseDto<string>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.StatusCode,
            Message = result.Message,
            Data = string.Empty
        };
    }
}