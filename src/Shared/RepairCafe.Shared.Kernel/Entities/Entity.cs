using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Kernel.Entities;

public abstract class Entity : IEntity
{
    public abstract object[] GetKeys();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        var keys = GetKeys();
        var otherKeys = other.GetKeys();

        return keys.Length == otherKeys.Length && keys.SequenceEqual(otherKeys);
    }

    public override int GetHashCode()
    {
        return GetKeys().Aggregate(default(int), HashCode.Combine);
    }
}

public abstract class Entity<TKey> : Entity, IEntity<TKey>
{
    public TKey Id { get; protected set; } = default!;

    protected Entity() { }

    protected Entity(TKey id)
    {
        Id = id;
    }

    public override object[] GetKeys()
    {
        return [Id!];
    }
}