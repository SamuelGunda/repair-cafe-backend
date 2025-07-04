namespace RepairCafe.Shared.Kernel.Abstractions;

using Microsoft.EntityFrameworkCore;

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(CancellationToken cancellationToken = default);
}