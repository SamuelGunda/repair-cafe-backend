using RepairCafe.Shared.Infrastructure.Outbox.Models;

namespace RepairCafe.Shared.Infrastructure.Outbox.Persistence;

public interface IOutboxMessageRepository
{
    void Add(OutboxMessage outboxMessage);
}
