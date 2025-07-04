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
        var code = StatusCodes.Status500InternalServerError;

        // if (exception is NotFoundException) code = StatusCodes.Status404NotFound;

        var response = new
        {
            error = exception.Message,
            stackTrace = isDevelopment ? exception.StackTrace : null
        };

        var result = System.Text.Json.JsonSerializer.Serialize(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code;
        
        return context.Response.WriteAsync(result);
    }
}