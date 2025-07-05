using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Infrastructure.Outbox.Configurations;
using RepairCafe.Shared.Infrastructure.Outbox.Models;
using RepairCafe.Shared.Infrastructure.Persistence;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure.Outbox.Processing;

public class OutboxMessageProcessorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMessageProcessorJob> _logger;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly OutboxSettings _settings;

    public OutboxMessageProcessorJob(
        IServiceProvider serviceProvider, 
        ILogger<OutboxMessageProcessorJob> logger, 
        JsonSerializerSettings serializerSettings,
        IOptions<OutboxSettings> outboxOptions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _serializerSettings = serializerSettings;
        _settings = outboxOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred in the outbox processor.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.PollingIntervalInSeconds), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting to process outbox messages.");

        using var scope = _serviceProvider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();
        var dbContexts = scope.ServiceProvider.GetServices<IOutboxDbContext>();

        foreach (var dbContext in dbContexts)
        {
            var messages = await dbContext.OutboxMessages
                .Where(m => m.Status == OutboxMessageStatus.Pending)
                .OrderBy(m => m.OccurredOnUtc)
                .Take(_settings.BatchSize)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    var domainEventObject = JsonConvert.DeserializeObject(
                        message.Content,
                        _serializerSettings);

                    if (domainEventObject is not IDomainEvent domainEvent)
                    {
                        _logger.LogWarning("Could not deserialize domain event from outbox message {MessageId}", message.Id);
                        message.Status = OutboxMessageStatus.Failed;
                        message.Error = "Deserialization failed";
                        continue;
                    }

                    await publisher.PublishAsync(domainEvent, stoppingToken);

                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Status = OutboxMessageStatus.Processed;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    message.Error = ex.Message;
                    message.RetryCount++;
                    message.Status = message.RetryCount >= _settings.MaxRetries 
                        ? OutboxMessageStatus.Failed 
                        : OutboxMessageStatus.Pending;
                }
            }

            if (messages.Any())
            {
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Processed {Count} messages from a DbContext.", messages.Count);
            }
        }

        _logger.LogInformation("Finished processing outbox messages.");
    }
}
