using Newtonsoft.Json.Serialization;

namespace RepairCafe.Shared.Infrastructure.Serialization;

public class KnownTypesSerializationBinder : ISerializationBinder
{
    private readonly IReadOnlyDictionary<string, Type> _knownTypes;

    public KnownTypesSerializationBinder(IEnumerable<Type> knownTypes)
    {
        _knownTypes = knownTypes.ToDictionary(t => t.FullName!, t => t);
    }

    public Type BindToType(string? assemblyName, string typeName)
    { 
        if (_knownTypes.TryGetValue(typeName, out var type))
        {
            return type;
        }

        throw new InvalidOperationException($"Type '{typeName}' is not a known type and cannot be deserialized.");
    }

    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    { 
        assemblyName = null;
        typeName = serializedType.FullName;
    }
}
