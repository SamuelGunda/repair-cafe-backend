using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Application.Abstractions;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
}
