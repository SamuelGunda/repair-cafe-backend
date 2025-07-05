using RepairCafe.Api.Middleware;

namespace RepairCafe.Api.Extensions;

public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        const string handlerAddedKey = "GlobalExceptionHandlerAdded";
        if (app.Properties.ContainsKey(handlerAddedKey))
        {
            return app;
        }

        var environment = app.ApplicationServices.GetService<IHostEnvironment>();
        var isDevelopment = environment?.IsDevelopment() ?? false;

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>(isDevelopment);
        app.Properties[handlerAddedKey] = true;

        return app;
    }
}
