using System.Diagnostics.CodeAnalysis;

namespace ECommerce.Api.Common.Results;

public sealed class Result<T> : Result
    where T : notnull
{
    private readonly T? _value;

    private Result(T value)
        : base(true, [])
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    private Result(IEnumerable<Error> errors)
        : base(false, errors)
    {
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    public static Result<T> Success(T value) => new(value);

    public new static Result<T> Failure(Error firstError, params Error[] additionalErrors)
    {
        ArgumentNullException.ThrowIfNull(firstError);
        ArgumentNullException.ThrowIfNull(additionalErrors);

        return new(additionalErrors.Prepend(firstError));
    }

    public new static Result<T> Failure(IEnumerable<Error> errors) => new(errors);

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value!;
        return IsSuccess;
    }

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(Value) : onFailure(Errors);
    }

    public void Switch(
        Action<T> onSuccess,
        Action<IReadOnlyList<Error>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
        {
            onSuccess(Value);
            return;
        }

        onFailure(Errors);
    }
}
