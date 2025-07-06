namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(CancellationToken cancellationToken = default);
}