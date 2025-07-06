using MediatR;

namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IDomainEvent  : INotification
{
    DateTime OccurredOn { get; }
}