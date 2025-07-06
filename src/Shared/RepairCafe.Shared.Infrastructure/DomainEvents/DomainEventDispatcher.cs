using Newtonsoft.Json;
using RepairCafe.Shared.Infrastructure.Outbox;
using RepairCafe.Shared.Infrastructure.Outbox.Models;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure.DomainEvents;

internal class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IOutboxMessageRepository _outboxMessageRepository;

    public DomainEventDispatcher(
        IUnitOfWork unitOfWork, 
        JsonSerializerSettings serializerSettings,
        IOutboxMessageRepository outboxMessageRepository)
    {
        _unitOfWork = unitOfWork;
        _serializerSettings = serializerSettings;
        _outboxMessageRepository = outboxMessageRepository;
    }

    public Task DispatchEventsAsync(CancellationToken cancellationToken = default)
    {
        var aggregateRoots = _unitOfWork.GetAggregatesWithDomainEvents();

        var domainEvents = aggregateRoots
            .SelectMany(x => x.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(
                    domainEvent,
                    _serializerSettings)
            };
            
            _outboxMessageRepository.Add(outboxMessage);
        }

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        return Task.CompletedTask;
    }
}