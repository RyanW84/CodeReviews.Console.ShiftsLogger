using System.Net;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Services;

public class WorkerService(IWorkerRepository workerRepository) : IWorkerService
{
    private readonly IWorkerRepository _workerRepository = workerRepository ?? throw new ArgumentNullException(nameof(workerRepository));

    public async Task<ApiResponseDto<List<Worker>>> GetAllWorkers(
        WorkerFilterOptions workerOptions
    )
    {
        var result = await _workerRepository.GetAllAsync(workerOptions).ConfigureAwait(false);
        return new ApiResponseDto<List<Worker>>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.IsFailure ? result.StatusCode : System.Net.HttpStatusCode.OK,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<Worker>> GetWorkerById(int id)
    {
        var result = await _workerRepository.GetByIdAsync(id).ConfigureAwait(false);
        if (result.IsFailure || result.Data is null)
            return new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = result.StatusCode,
                Message = result.Message,
                Data = null
            };

        return new ApiResponseDto<Worker>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<Worker>> CreateWorker(WorkerApiRequestDto worker)
    {
        try
        {
            var result = await _workerRepository.CreateAsync(worker).ConfigureAwait(false);
            if (result.IsFailure)
                return new ApiResponseDto<Worker>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                };

            return new ApiResponseDto<Worker>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.Created,
                Message = result.Message,
                Data = result.Data
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Back end worker service - {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Worker?>> UpdateWorker(
        int id,
        WorkerApiRequestDto updatedWorker
    )
    {
        var result = await _workerRepository.UpdateAsync(id, updatedWorker).ConfigureAwait(false);
        if (result.IsFailure)
            return new ApiResponseDto<Worker?>
            {
                RequestFailed = true,
                ResponseCode = result.StatusCode,
                Message = result.Message,
                Data = null
            };

        return new ApiResponseDto<Worker?>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<ApiResponseDto<string?>> DeleteWorker(int id)
    {
        var result = await _workerRepository.DeleteAsync(id).ConfigureAwait(false);
        return new ApiResponseDto<string?>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.StatusCode,
            Message = result.Message,
            Data = null
        };
    }
}