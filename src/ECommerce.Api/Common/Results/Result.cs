using System.Collections.ObjectModel;

namespace ECommerce.Api.Common.Results;

public class Result
{
    private readonly ReadOnlyCollection<Error> _errors;

    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        Error[] errorArray = errors.ToArray();

        if (errorArray.Any(static error => error is null))
        {
            throw new ArgumentException("A result cannot contain a null error.", nameof(errors));
        }

        if (isSuccess && errorArray.Length > 0)
        {
            throw new ArgumentException("A successful result cannot contain errors.", nameof(errors));
        }

        if (!isSuccess && errorArray.Length == 0)
        {
            throw new ArgumentException("A failed result must contain at least one error.", nameof(errors));
        }

        if (!isSuccess && errorArray.Select(static error => error.Type).Distinct().Skip(1).Any())
        {
            throw new ArgumentException(
                "All errors in a result must have the same error type.",
                nameof(errors));
        }

        IsSuccess = isSuccess;
        _errors = Array.AsReadOnly(errorArray);
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<Error> Errors => _errors;

    public static Result Success() => new(true, []);

    public static Result Failure(Error firstError, params Error[] additionalErrors)
    {
        ArgumentNullException.ThrowIfNull(firstError);
        ArgumentNullException.ThrowIfNull(additionalErrors);

        return new(false, additionalErrors.Prepend(firstError));
    }

    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);

    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess() : onFailure(Errors);
    }

    public void Switch(
        Action onSuccess,
        Action<IReadOnlyList<Error>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
        {
            onSuccess();
            return;
        }

        onFailure(Errors);
    }
}
