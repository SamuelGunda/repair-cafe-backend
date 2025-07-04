using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RepairCafe.Shared.Infrastructure.Behaviors;
using RepairCafe.Shared.Infrastructure.DomainEvents;
using RepairCafe.Shared.Infrastructure.Middleware;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetService<IHostEnvironment>();
        var isDevelopment = environment?.IsDevelopment() ?? false;
        
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>(isDevelopment);
    }
}