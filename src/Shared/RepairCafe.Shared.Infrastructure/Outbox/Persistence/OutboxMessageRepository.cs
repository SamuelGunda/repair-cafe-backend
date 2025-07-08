using RepairCafe.Shared.Infrastructure.Outbox.Models;
using RepairCafe.Shared.Infrastructure.Persistence;

namespace RepairCafe.Shared.Infrastructure.Outbox.Persistence;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly IOutboxDbContext _dbContext;

    public OutboxMessageRepository(IOutboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(OutboxMessage outboxMessage)
    {
        _dbContext.OutboxMessages.Add(outboxMessage);
    }
}
