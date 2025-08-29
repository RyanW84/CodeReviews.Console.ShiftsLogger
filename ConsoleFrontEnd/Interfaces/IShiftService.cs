using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Interfaces;

public interface IShiftService
{
    Task<ApiResponseDto<List<Shift>>> GetAllShiftsAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResponseDto<Shift?>> GetShiftByIdAsync(int id);
    Task<ApiResponseDto<Shift>> CreateShiftAsync(Shift shift);
    Task<ApiResponseDto<Shift?>> UpdateShiftAsync(int id, Shift updatedShift);
    Task<ApiResponseDto<bool>> DeleteShiftAsync(int id);
    Task<ApiResponseDto<List<Shift>>> GetShiftsByFilterAsync(Models.FilterOptions.ShiftFilterOptions filter, int pageNumber = 1, int pageSize = 10);
}
