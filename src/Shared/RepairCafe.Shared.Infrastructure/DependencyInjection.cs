using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Infrastructure.BackgroundJobs;
using RepairCafe.Shared.Infrastructure.Behaviors;
using RepairCafe.Shared.Infrastructure.DomainEvents;
using RepairCafe.Shared.Infrastructure.Middleware;

namespace RepairCafe.Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        services.AddScoped<IDomainEventPublisher, InProcessDomainEventPublisher>();

        services.AddHostedService<OutboxMessageProcessorJob>();

        return services;
    }

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