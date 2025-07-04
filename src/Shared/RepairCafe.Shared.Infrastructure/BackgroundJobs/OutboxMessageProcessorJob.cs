using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Infrastructure.Persistence;
using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure.BackgroundJobs;

public class OutboxMessageProcessorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMessageProcessorJob> _logger;
    private readonly JsonSerializerSettings _serializerSettings;

    public OutboxMessageProcessorJob(
        IServiceProvider serviceProvider, 
        ILogger<OutboxMessageProcessorJob> logger, 
        JsonSerializerSettings serializerSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _serializerSettings = serializerSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessOutboxMessagesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
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
            var context = (DbContext)dbContext;
            var messages = await dbContext.OutboxMessages
                .Where(m => m.Status == "Pending")
                .OrderBy(m => m.OccurredOnUtc)
                .Take(20)
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
                        message.Status = "Failed";
                        message.Error = "Deserialization failed";
                        continue;
                    }

                    await publisher.PublishAsync(domainEvent, stoppingToken);

                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Status = "Processed";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    message.Error = ex.Message;
                    message.RetryCount++;
                    message.Status = "Failed";
                }
            }

            if (messages.Any())
            {
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Processed {Count} messages from a DbContext.", messages.Count);
            }
        }

        _logger.LogInformation("Finished processing outbox messages.");
    }
}
