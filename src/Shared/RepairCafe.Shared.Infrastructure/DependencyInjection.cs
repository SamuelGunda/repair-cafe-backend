using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Infrastructure.Behaviors;
using RepairCafe.Shared.Infrastructure.DomainEvents;
using RepairCafe.Shared.Infrastructure.Serialization;
using RepairCafe.Shared.Kernel.Abstractions;
using System.Reflection;
using RepairCafe.Shared.Infrastructure.Integration;
using RepairCafe.Shared.Infrastructure.Outbox.Configurations;
using RepairCafe.Shared.Infrastructure.Outbox.Processing;
using RepairCafe.Shared.Infrastructure.Outbox.Persistence;

namespace RepairCafe.Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
        , IEnumerable<Assembly>? domainAssemblies = null)
    {
        services.Configure<OutboxSettings>(
            configuration.GetSection(OutboxSettings.SectionName));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        services.AddScoped<IDomainEventPublisher, InProcessDomainEventPublisher>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();

        services.AddHostedService<OutboxMessageProcessorJob>();

        var domainEventTypes = (domainAssemblies ?? Directory.GetFiles(AppContext.BaseDirectory, "RepairCafe.*.Domain.dll")
                .Select(Assembly.LoadFrom))
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
}