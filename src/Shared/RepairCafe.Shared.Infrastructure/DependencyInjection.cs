using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Infrastructure.BackgroundJobs;
using RepairCafe.Shared.Infrastructure.Behaviors;
using RepairCafe.Shared.Infrastructure.DomainEvents;
using RepairCafe.Shared.Infrastructure.Middleware;
using RepairCafe.Shared.Infrastructure.Serialization;
using RepairCafe.Shared.Kernel.Abstractions;
using System.Reflection;

namespace RepairCafe.Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutboxSettings>(
            configuration.GetSection(OutboxSettings.SectionName));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        services.AddScoped<IDomainEventPublisher, InProcessDomainEventPublisher>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddHostedService<OutboxMessageProcessorJob>();

        var domainEventTypes = Directory.GetFiles(AppContext.BaseDirectory, "RepairCafe.*.Domain.dll")
            .Select(Assembly.LoadFrom)
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToList();

        services.AddSingleton<ISerializationBinder>(new KnownTypesSerializationBinder(domainEventTypes));

        services.AddSingleton(sp =>
        {
            var binder = sp.GetRequiredService<ISerializationBinder>();
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = binder
            };
        });

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