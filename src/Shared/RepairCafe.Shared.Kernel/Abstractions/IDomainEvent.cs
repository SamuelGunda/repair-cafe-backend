namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}