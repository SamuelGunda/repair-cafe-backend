using MediatR;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure.DomainEvents;

public class InProcessDomainEventPublisher : IDomainEventPublisher
{
    private readonly IMediator _mediator;

    public InProcessDomainEventPublisher(IMediator mediator) => _mediator = mediator;

    public Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default) =>
        _mediator.Publish(@event, cancellationToken);
}
