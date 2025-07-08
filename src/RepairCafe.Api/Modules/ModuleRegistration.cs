using RepairCafe.Modules.Auth.Infrastructure;
using RepairCafe.Shared.Infrastructure.ModuleRegistry;

namespace RepairCafe.Api.Modules;

public static class ModuleRegistration
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthInfrastructure(configuration);
        
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly);
            
            foreach (var assembly in ModuleRegistry.ApplicationAssemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }
        });
        
        return services;
    }
}
