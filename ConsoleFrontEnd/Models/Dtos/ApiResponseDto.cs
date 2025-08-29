using System.Net;

namespace ConsoleFrontEnd.Models.Dtos;

public class ApiResponseDto<T> // Generic <T> means the output can take the shape of different data types
    (string message)
{
    public bool RequestFailed { get; set; }
    public HttpStatusCode ResponseCode { get; set; }
    public string? Message { get; set; } = message;
    public T? Data { get; set; } // Nullable type to allow for no data to be returned
    public int TotalCount { get; set; }

    // Pagination properties
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}