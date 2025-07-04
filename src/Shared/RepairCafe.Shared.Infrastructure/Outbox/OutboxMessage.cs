using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Infrastructure.Outbox;

public static class OutboxMessageStatus
{
    public const string Pending = "Pending";
    public const string Processed = "Processed";
    public const string Failed = "Failed";
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; } 
    public string Status { get; set; }
}
