namespace RepairCafe.Shared.Kernel.Abstractions;

public interface IResult
{
    bool IsSuccess { get; }
    string? Error { get; }
}

public interface IResult<out T> : IResult
{
    T? Value { get; }
}