using Microsoft.EntityFrameworkCore;
using RepairCafe.Shared.Infrastructure.Outbox;

namespace RepairCafe.Shared.Infrastructure.Persistence;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
}
