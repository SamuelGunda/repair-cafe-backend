using MediatR;

namespace RepairCafe.Shared.Application.Abstractions;

public interface IQuery
{
}

public interface IQuery<out TResponse> : IQuery, IRequest<TResponse>
{
}
