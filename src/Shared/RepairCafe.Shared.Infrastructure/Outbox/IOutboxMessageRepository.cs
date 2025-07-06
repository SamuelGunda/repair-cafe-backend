using RepairCafe.Shared.Infrastructure.Outbox.Models;

namespace RepairCafe.Shared.Infrastructure.Outbox;

public interface IOutboxMessageRepository
{
    void Add(OutboxMessage outboxMessage);
}
