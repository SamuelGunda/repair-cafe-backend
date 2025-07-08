using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Infrastructure.Outbox.Models;
using RepairCafe.Shared.Infrastructure.Outbox.Persistence;

namespace RepairCafe.Shared.Infrastructure.Integration;

public class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly ILogger<OutboxIntegrationEventPublisher> _logger;

    public OutboxIntegrationEventPublisher(
        IOutboxMessageRepository outboxMessageRepository,
        JsonSerializerSettings serializerSettings,
        ILogger<OutboxIntegrationEventPublisher> logger)
    {
        _outboxMessageRepository = outboxMessageRepository;
        _serializerSettings = serializerSettings;
        _logger = logger;
    }

    public Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = @event.GetType().Name,
                Content = JsonConvert.SerializeObject(@event, _serializerSettings)
            };

            _outboxMessageRepository.Add(outboxMessage);
            
            _logger.LogInformation("Integration event {EventName} added to outbox with ID {MessageId}", 
                @event.GetType().Name, outboxMessage.Id);
                
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish integration event {EventName}", @event.GetType().Name);
            throw;
        }
    }
}
