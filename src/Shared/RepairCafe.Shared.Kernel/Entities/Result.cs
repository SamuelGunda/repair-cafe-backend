using RepairCafe.Shared.Kernel.Abstractions;

namespace RepairCafe.Shared.Kernel.Entities;

public class Result : IResult
{
    public bool IsSuccess { get; }
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);

    public static Result<T> NotFound<T>(string code, string description) => 
        new(default, false, new Error(code, description, ErrorType.NotFound));

    public static Result<T> Conflict<T>(string code, string description) => 
        new(default, false, new Error(code, description, ErrorType.Conflict));
}

public class Result<T> : Result, IResult<T>
{
    public T? Value { get; }

    protected internal Result(T? value, bool isSuccess, Error error) 
        : base(isSuccess, error)
    {
        Value = value;
    }
}