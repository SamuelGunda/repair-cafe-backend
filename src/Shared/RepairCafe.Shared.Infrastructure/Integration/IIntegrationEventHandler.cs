using MediatR;
using RepairCafe.Shared.Application.Abstractions;

namespace RepairCafe.Shared.Infrastructure.Integration;

public interface IIntegrationEventHandler<in TIntegrationEvent> : INotificationHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
}