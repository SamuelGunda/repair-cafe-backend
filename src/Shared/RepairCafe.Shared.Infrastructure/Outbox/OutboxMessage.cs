using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public string Status { get; set; }
}
