namespace RepairCafe.Shared.Kernel.Abstractions;

using Microsoft.EntityFrameworkCore;

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(DbContext context, CancellationToken cancellationToken = default);
}