namespace Shared;

public class Result
{
    protected Result(bool isSuccess, string[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<string> Errors { get; }

    public static Result Ok() => new(true, []);

    public static Result Fail(params string[] errors) => new(false, errors);

    public static Result<T> Ok<T>(T value) => new(value);

    public static Result<T> Fail<T>(params string[] errors) => new(errors);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T value)
        : base(true, []) => _value = value;

    internal Result(string[] errors)
        : base(false, errors) { }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<IReadOnlyList<string>, TOut> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Errors);

    public static implicit operator Result<T>(T value) => new(value);
}
