using RepairCafe.Shared.Kernel.Entities;

namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IRepository<T, in TKey> where T : class, IAggregateRoot, IEntity<TKey>
{
    void Add(T entity);
    void Remove(T entity);
    Task<Result<T>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
}