using Microsoft.EntityFrameworkCore;
using RepairCafe.Shared.Infrastructure.Outbox.Models;

namespace RepairCafe.Shared.Infrastructure.Persistence;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
