using System.Reflection;

namespace RepairCafe.Api.Modules;

public static class ModuleRegistry
{
    private static readonly List<Assembly> _domainAssemblies = [];
    private static readonly List<Assembly> _applicationAssemblies = [];

    public static IEnumerable<Assembly> DomainAssemblies => _domainAssemblies.AsReadOnly();
    public static IEnumerable<Assembly> ApplicationAssemblies => _applicationAssemblies.AsReadOnly();

    public static void RegisterDomainAssembly(Assembly assembly)
    {
        if (!_domainAssemblies.Contains(assembly))
        {
            _domainAssemblies.Add(assembly);
        }
    }

    public static void RegisterApplicationAssembly(Assembly assembly)
    {
        if (!_applicationAssemblies.Contains(assembly))
        {
            _applicationAssemblies.Add(assembly);
        }
    }
}
