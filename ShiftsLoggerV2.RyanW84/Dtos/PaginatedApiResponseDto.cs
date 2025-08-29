using System.Net;

namespace ShiftsLoggerV2.RyanW84.Dtos;

public class PaginatedApiResponseDto<T>
{
    public bool RequestFailed { get; set; }
    public HttpStatusCode ResponseCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
