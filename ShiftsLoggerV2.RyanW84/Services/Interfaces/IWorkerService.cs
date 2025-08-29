using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;

namespace ShiftsLoggerV2.RyanW84.Services;

public interface IWorkerService
{
    public Task<ApiResponseDto<List<Worker>>> GetAllWorkers(WorkerFilterOptions workerOptions);
    public Task<ApiResponseDto<Worker>> GetWorkerById(int id);
    public Task<ApiResponseDto<Worker>> CreateWorker(WorkerApiRequestDto worker);
    public Task<ApiResponseDto<Worker?>> UpdateWorker(int id, WorkerApiRequestDto updatedWorker);
    public Task<ApiResponseDto<string?>> DeleteWorker(int id);
}