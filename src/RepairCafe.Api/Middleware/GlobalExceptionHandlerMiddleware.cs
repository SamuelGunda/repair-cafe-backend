using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace RepairCafe.Api.Middleware;

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

    private static async Task HandleExceptionAsync(
        HttpContext context, 
        Exception exception, 
        bool isDevelopment)
    {
        (var statusCode, object response) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                new ValidationProblemDetails(
                    validationException.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        ))
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest
                }),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = "You are not authorized to perform this action."
                }),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred",
                    Detail = isDevelopment ? exception.ToString() : "An internal server error has occurred."
                })
        };
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(response);
    }
}
