// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

public readonly ref struct TypedValueError(
    TypedValueError.ErrorTypeEnum errorType,
    string error)
    : IError
{
    public string Message { get; } = error;
    public ErrorTypeEnum ErrorType { get; } = errorType;

    public static TypedValueError General(string error)
        => new(ErrorTypeEnum.General, error);

    public static TypedValueError OutOfRange(string error)
        => new(ErrorTypeEnum.OutOfRange, error);

    public static TypedValueError InvalidOperation(string error)
        => new(ErrorTypeEnum.InvalidOperation, error);

    public static TypedValueError Unreachable(string error)
        => new(ErrorTypeEnum.Unreachable, error);

    public static ValueResult<TValue, TypedValueError> General<TValue>(string error)
        where TValue : allows ref struct
        => ValueResult.Failure<TValue, TypedValueError>(General(error));

    public static ValueResult<TValue, TypedValueError> OutOfRange<TValue>(string error)
        where TValue : allows ref struct
        => ValueResult.Failure<TValue, TypedValueError>(OutOfRange(error));

    public static ValueResult<TValue, TypedValueError> InvalidOperation<TValue>(string error)
        where TValue : allows ref struct
        => ValueResult.Failure<TValue, TypedValueError>(InvalidOperation(error));

    public static ValueResult<TValue, TypedValueError> Unreachable<TValue>(string error)
        where TValue : allows ref struct
        => ValueResult.Failure<TValue, TypedValueError>(Unreachable(error));

    public enum ErrorTypeEnum
    {
        General,
        OutOfRange,
        InvalidOperation,

        Unreachable,
    }
}

[StructLayout(LayoutKind.Auto)]
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
        bool isSuccess,
        scoped in TValue? value,
        scoped in TError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static ValueResult<TValue, TError> Success(
        scoped in TValue value)
        => new(
            isSuccess: true,
            value: value,
            error: default);

    public static ValueResult<TValue, TError> Failure(
        scoped in TError error)
        => new(
            isSuccess: false,
            value: default,
            error: error);
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
        scoped in TValue value)
        where TValue : allows ref struct
        => ValueResult<TValue, ValueError>.Success(value);

    public static ValueResult<TValue, ValueError> Failure<TValue>(
        string error)
        where TValue : allows ref struct
        => ValueResult<TValue, ValueError>.Failure(new(error));
}

public static class ValueResultExtensions
{
    public delegate TValue2 MapFunc<TValue1, TValue2>(
        scoped in TValue1 value)
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

    public static ValueResult<TValue, TNewError> MapError<TValue, TError, TNewError>(
        this scoped in ValueResult<TValue, TError> result,
        Func<TError, TNewError> map)
        where TError : IError, allows ref struct
        where TNewError : IError, allows ref struct
        where TValue : allows ref struct
        => result.IsSuccess
            ? ValueResult<TValue, TNewError>.Success(result.Value)
            : ValueResult<TValue, TNewError>.Failure(map(result.Error));

    public static TResult Match<TValue, TError, TResult>(
        this scoped in ValueResult<TValue, TError> result,
        Func<TValue, TResult> onSuccess,
        Func<TError, TResult> onFailure)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    public delegate TResult OnSuccessFunc<TValue, TResult>(
        scoped in TValue value)
        where TValue : allows ref struct;

    public delegate TResult OnFailureFunc<TError, TResult>(
        scoped in TError error)
        where TError : IError, allows ref struct;

    public static TResult MatchWithDelegate<TValue, TError, TResult>(
        this scoped in ValueResult<TValue, TError> result,
        OnSuccessFunc<TValue, TResult> onSuccess,
        OnFailureFunc<TError, TResult> onFailure)
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
            ? ValueResult.Failure<TValue>(
                $"{paramName} cannot be zero")
            : ValueResult.Success(value);

    public static ValueResult<TValue, ValueError> ErrorIfGreaterThen<TValue>(
        this scoped in ValueResult<TValue, ValueError> value,
        scoped in TValue other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : IComparable<TValue>
        => value.Bind(
            other,
            paramName,
            ErrorIfGreaterThen);

    public static ValueResult<TValue, ValueError> ErrorIfGreaterThen<TValue>(
        scoped in TValue value,
        scoped in TValue other,
        [CallerArgumentExpression(nameof(value))] scoped in string? paramName = null)
        where TValue : IComparable<TValue>
        => value.CompareTo(other) > 0
            ? ValueResult.Failure<TValue>(
                $"{paramName} cannot be greater than {other}")
            : ValueResult.Success(value);
}