using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RepairCafe.Shared.Infrastructure.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly bool _isDevelopment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        bool isDevelopment)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = isDevelopment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(httpContext, ex, _isDevelopment);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                new
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                }),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                (object)new
                {
                    Title = "Forbidden",
                    Status = StatusCodes.Status403Forbidden,
                    Detail = "You are not authorized to perform this action."
                }),
            _ => (
                StatusCodes.Status500InternalServerError,
                (object)new
                {
                    Title = "An error occurred",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = exception.Message,
                    StackTrace = isDevelopment ? exception.StackTrace : null
                })
        };

        var result = Newtonsoft.Json.JsonConvert.SerializeObject(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(result);
    }
}