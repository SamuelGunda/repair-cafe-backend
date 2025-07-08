using System.Collections.Concurrent;
using System.Reflection;

namespace RepairCafe.Shared.Infrastructure.ModuleRegistry;

public static class ModuleRegistry
{
    private static readonly ConcurrentDictionary<Assembly, byte> _domainAssemblies = new();
    private static readonly ConcurrentDictionary<Assembly, byte> _applicationAssemblies = new();

    public static IEnumerable<Assembly> DomainAssemblies => _domainAssemblies.Keys;
    public static IEnumerable<Assembly> ApplicationAssemblies => _applicationAssemblies.Keys;

    public static void RegisterDomainAssembly(Assembly assembly)
    {
        _domainAssemblies.TryAdd(assembly, 0);
    }

    public static void RegisterApplicationAssembly(Assembly assembly)
    {
        _applicationAssemblies.TryAdd(assembly, 0);
    }
}
