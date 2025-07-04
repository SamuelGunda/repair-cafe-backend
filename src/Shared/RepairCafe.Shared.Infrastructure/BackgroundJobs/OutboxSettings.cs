namespace RepairCafe.Shared.Infrastructure.BackgroundJobs;

public class OutboxSettings
{
    public const string SectionName = "Outbox";

    public int PollingIntervalInSeconds { get; set; } = 10;
    public int BatchSize { get; set; } = 20;
    public int MaxRetries { get; set; } = 3;
}
