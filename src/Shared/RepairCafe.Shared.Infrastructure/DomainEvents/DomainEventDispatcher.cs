using Microsoft.Extensions.Logging;
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
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IUnitOfWork unitOfWork, 
        JsonSerializerSettings serializerSettings,
        IOutboxMessageRepository outboxMessageRepository,
        ILogger<DomainEventDispatcher> logger)
    {
        _unitOfWork = unitOfWork;
        _serializerSettings = serializerSettings;
        _outboxMessageRepository = outboxMessageRepository;
        _logger = logger;
    }

    public Task DispatchEventsAsync(CancellationToken cancellationToken = default)
    {
        var aggregateRoots = _unitOfWork.GetAggregatesWithDomainEvents();

        var domainEvents = aggregateRoots
            .SelectMany(x => x.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to serialize domain event {EventName} for outbox.", domainEvent.GetType().Name);
            }
        }

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        return Task.CompletedTask;
    }
}