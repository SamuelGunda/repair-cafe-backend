using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Kernel.Entities;

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}