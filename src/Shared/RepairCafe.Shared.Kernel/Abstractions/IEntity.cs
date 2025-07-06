namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IEntity
{
    object[] GetKeys();
}

public interface IEntity<out TKey> : IEntity
{
    TKey Id { get; }
}