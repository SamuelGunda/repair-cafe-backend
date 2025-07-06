using RepairCafe.Shared.Kernel.Entities;

namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IResult
{
    bool IsSuccess { get; }
    Error Error { get; }
}

public interface IResult<out T> : IResult
{
    T? Value { get; }
}