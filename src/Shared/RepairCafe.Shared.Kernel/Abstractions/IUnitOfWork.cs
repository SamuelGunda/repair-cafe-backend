namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IUnitOfWork
{
    IEnumerable<IAggregateRoot> GetAggregatesWithDomainEvents();
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}