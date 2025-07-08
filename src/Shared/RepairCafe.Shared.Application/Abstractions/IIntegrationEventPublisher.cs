namespace RepairCafe.Shared.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default);
}
