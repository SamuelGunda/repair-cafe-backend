namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IRepository<T> where T : class, IAggregateRoot
{
    void Add(T entity);
    void Remove(T entity);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}