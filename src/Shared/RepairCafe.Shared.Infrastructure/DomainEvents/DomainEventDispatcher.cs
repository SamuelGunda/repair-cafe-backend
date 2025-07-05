using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RepairCafe.Shared.Infrastructure.Outbox;
using RepairCafe.Shared.Infrastructure.Persistence;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure.DomainEvents;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JsonSerializerSettings _serializerSettings;
    

    public DomainEventDispatcher(IUnitOfWork unitOfWork, JsonSerializerSettings serializerSettings)
    {
        _unitOfWork = unitOfWork;
        _serializerSettings = serializerSettings;
    }

    public Task DispatchEventsAsync(CancellationToken cancellationToken = default)
    {
        if (_unitOfWork is not DbContext dbContext)
        {
            return Task.CompletedTask;
        }

        var domainEntities = dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEventObjects = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        if (_unitOfWork is IOutboxDbContext outboxDbContext)
        {
            foreach (var outboxMessage in domainEventObjects.Select(domainEvent => new OutboxMessage
                     {
                         Id = Guid.NewGuid(),
                         OccurredOnUtc = DateTime.UtcNow,
                         Type = domainEvent.GetType().Name,
                         Content = JsonConvert.SerializeObject(
                             domainEvent,
                             _serializerSettings)
                     }))
            {
                outboxDbContext.OutboxMessages.Add(outboxMessage);
            }
        }

        foreach (var entity in domainEntities)
        {
            entity.Entity.ClearDomainEvents();
        }

        return Task.CompletedTask;
    }
}