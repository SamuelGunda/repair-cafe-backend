using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RepairCafe.Shared.Infrastructure.Outbox;
using RepairCafe.Shared.Infrastructure.Persistence;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure.DomainEvents;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly DbContext _context;

    public DomainEventDispatcher(DbContext context)
    {
        _context = context;
    }

    public Task DispatchEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = _context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        if (_context is IOutboxDbContext outboxDbContext)
        {
            foreach (var domainEvent in domainEvents)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOnUtc = DateTime.UtcNow,
                    Type = domainEvent.GetType().Name,
                    Content = JsonConvert.SerializeObject(
                        domainEvent,
                        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
                };
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