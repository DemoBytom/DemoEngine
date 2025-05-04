// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Demo.Tools.Common;

public interface IResult<out TValue, out TError>
    where TError : IError, allows ref struct
    where TValue : allows ref struct
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    bool IsSuccess { get; }

    TValue? Value { get; }
    TError? Error { get; }
}

public interface IError
{
    string Message { get; }
}

public readonly ref struct ValueError(
    string error)
    : IError
{
    public string Message { get; } = error;
}

public readonly ref struct ValueResult<TValue, TError>
    : IResult<TValue, TError>
    where TError : IError, allows ref struct
    where TValue : allows ref struct
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public TValue? Value { get; }
    public TError? Error { get; }

    private ValueResult(
        bool isSuccess, TValue? value, TError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static ValueResult<TValue, TError> Success(
        TValue value) => new(
            isSuccess: true,
            value: value,
            error: default);

    public static ValueResult<TValue, TError> Failure(
        TError error) => new(
            isSuccess: false,
            value: default,
            error: error);

    public static ValueResult<TValue, ValueError> Failure(
        string error) => new(
            isSuccess: false,
            value: default,
            error: new ValueError(error));
}

public static class ValueResult
{
    public static ValueResult<TValue, TError> Success<TValue, TError>(
        TValue value)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
        => ValueResult<TValue, TError>.Success(value);

    public static ValueResult<TValue, TError> Failure<TValue, TError>(
        TError error)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
        => ValueResult<TValue, TError>.Failure(error);

    public static ValueResult<TValue, ValueError> Success<TValue>(
        TValue value)
        where TValue : allows ref struct
        => ValueResult<TValue, ValueError>.Success(value);

    public static ValueResult<TValue, ValueError> Failure<TValue>(
        string error)
        where TValue : allows ref struct
        => ValueResult<TValue, ValueError>.Failure(error);
}

public static class ValueResultExtensions
{
    public delegate TValue2 MapFunc<TValue1, TValue2>(
        scoped in TValue1 value)
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct;

    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError>(
        scoped in TValue1 value)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct;

    public static ValueResult<TValue2, TError> Map<TValue1, TValue2, TError>(
        this scoped in ValueResult<TValue1, TError> result,
        MapFunc<TValue1, TValue2> map)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        => result.IsSuccess
            ? ValueResult<TValue2, TError>.Success(map(result.Value))
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError>(
        this scoped in ValueResult<TValue1, TError> result,
        BindFunc<TValue1, TValue2, TError> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue, TError> MapError<TValue, TError>(
        this scoped in ValueResult<TValue, TError> result,
        Func<TError, TError> map)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
        => result.IsSuccess
            ? ValueResult<TValue, TError>.Success(result.Value)
            : ValueResult<TValue, TError>.Failure(map(result.Error));

    public static TResult Match<TValue, TError, TResult>(
        this scoped in ValueResult<TValue, TError> result,
        Func<TValue, TResult> onSuccess,
        Func<TError, TResult> onFailure)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    public static ValueResult<TValue, ValueError> ErrorIfZero<TValue>(
        scoped in TValue value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : INumberBase<TValue>
        => TValue.IsZero(value)
            ? ValueResult<TValue, ValueError>.Failure(
                new ValueError($"{paramName} cannot be zero"))
            : ValueResult<TValue, ValueError>.Success(value);

    public static ValueResult<TValue, ValueError> ErrorIfGreaterThen<TValue>(
        this scoped in ValueResult<TValue, ValueError> value,
        TValue other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : IComparable<TValue>
    {
        return value.Bind(
                ErrorIfGreaterThenInner);

        ValueResult<TValue, ValueError> ErrorIfGreaterThenInner(
            scoped in TValue v)
            => ErrorIfGreaterThen(v, other, paramName);
    }

    public static ValueResult<TValue, ValueError> ErrorIfGreaterThen<TValue>(
        scoped in TValue value,
        TValue other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : IComparable<TValue>
        => value.CompareTo(other) > 0
            ? ValueResult<TValue, ValueError>.Failure(
                new ValueError($"{paramName} cannot be greater than {other}"))
            : ValueResult<TValue, ValueError>.Success(value);
}