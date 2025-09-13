using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;

namespace ShiftsLoggerV2.RyanW84.Filters;

/// <summary>
/// Action filter to handle exceptions centrally and return consistent error responses
/// </summary>
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "An unhandled exception occurred in {Controller}.{Action}",
            context.RouteData.Values["controller"], context.RouteData.Values["action"]);

        var (status, message) = ErrorMapper.Map(context.Exception);

        // Create appropriate response based on the expected return type
        var result = CreateErrorResponse(context, status, message);

        context.Result = result;
        context.ExceptionHandled = true;
    }

    private IActionResult CreateErrorResponse(ExceptionContext context, HttpStatusCode status, string message)
    {
        var actionDescriptor = context.ActionDescriptor;
        var returnType = actionDescriptor.Parameters
            .FirstOrDefault(p => p.ParameterType.IsGenericType &&
                                p.ParameterType.GetGenericTypeDefinition() == typeof(ActionResult<>))?
            .ParameterType.GetGenericArguments().FirstOrDefault();

        if (returnType == null)
        {
            // Fallback for actions without generic ActionResult
            return new ObjectResult(new ApiResponseDto<object>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null,
                TotalCount = 0,
            })
            {
                StatusCode = (int)status
            };
        }

        // Handle different response types
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(PaginatedApiResponseDto<>))
        {
            var dataType = returnType.GetGenericArguments()[0];
            var responseType = typeof(PaginatedApiResponseDto<>).MakeGenericType(dataType);
            var response = Activator.CreateInstance(responseType);

            // Set common properties using reflection
            responseType.GetProperty("RequestFailed")?.SetValue(response, true);
            responseType.GetProperty("ResponseCode")?.SetValue(response, status);
            responseType.GetProperty("Message")?.SetValue(response, message);
            responseType.GetProperty("Data")?.SetValue(response, null);
            responseType.GetProperty("TotalCount")?.SetValue(response, 0);

            // Try to set pagination properties if they exist
            var filterOptions = GetFilterOptionsFromContext(context);
            if (filterOptions != null)
            {
                responseType.GetProperty("PageNumber")?.SetValue(response, filterOptions.PageNumber);
                responseType.GetProperty("PageSize")?.SetValue(response, filterOptions.PageSize);
            }

            return new ObjectResult(response) { StatusCode = (int)status };
        }
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ApiResponseDto<>))
        {
            var dataType = returnType.GetGenericArguments()[0];
            var responseType = typeof(ApiResponseDto<>).MakeGenericType(dataType);
            var response = Activator.CreateInstance(responseType);

            // Set common properties using reflection
            responseType.GetProperty("RequestFailed")?.SetValue(response, true);
            responseType.GetProperty("ResponseCode")?.SetValue(response, status);
            responseType.GetProperty("Message")?.SetValue(response, message);
            responseType.GetProperty("Data")?.SetValue(response, null);
            responseType.GetProperty("TotalCount")?.SetValue(response, 0);

            return new ObjectResult(response) { StatusCode = (int)status };
        }
        else
        {
            // Fallback for other types
            return new ObjectResult(new ApiResponseDto<object>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null,
                TotalCount = 0,
            })
            {
                StatusCode = (int)status
            };
        }
    }

    private Models.FilterOptions.BaseFilterOptions? GetFilterOptionsFromContext(ExceptionContext context)
    {
        // Try to find filter options in action parameters
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (parameter.ParameterType.IsSubclassOf(typeof(Models.FilterOptions.BaseFilterOptions)))
            {
                var paramValue = context.HttpContext.Request.Query
                    .FirstOrDefault(q => q.Key.ToLower() == parameter.Name.ToLower()).Value;

                if (paramValue.Any())
                {
                    // Simple parsing for pageNumber and pageSize
                    var filterOptions = Activator.CreateInstance(parameter.ParameterType) as Models.FilterOptions.BaseFilterOptions;
                    if (filterOptions != null)
                    {
                        if (int.TryParse(context.HttpContext.Request.Query["pageNumber"], out var pageNumber))
                            filterOptions.PageNumber = pageNumber;
                        if (int.TryParse(context.HttpContext.Request.Query["pageSize"], out var pageSize))
                            filterOptions.PageSize = pageSize;
                        return filterOptions;
                    }
                }
            }
        }
        return null;
    }
}
