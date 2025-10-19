// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Demo.Tools.Common.ValueResults;

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
        where TResult : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    public static void Match<TValue, TError>(
        this scoped in ValueResult<TValue, TError> result,
        Action<TValue> onSuccess,
        Action<TError> onFailure)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
    {
        if (result.IsSuccess)
        {
            onSuccess(result.Value);
        }
        else
        {
            onFailure(result.Error);
        }
    }

    public static void MatchFailure<TValue, TError>(
        this scoped in ValueResult<TValue, TError> result,
        Action<TError> onFailure)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
    {
        if (result.IsSuccess)
        {
            return;
        }

        onFailure(result.Error);
    }

    public delegate TResult OnSuccessFunc<TValue, TResult>(
        scoped in TValue value)
        where TValue : allows ref struct
        where TResult : allows ref struct;

    public delegate TResult OnFailureFunc<TError, TResult>(
        scoped in TError error)
        where TError : IError, allows ref struct
        where TResult : allows ref struct;

    public delegate TResult OnSuccessFunc<TValue, TParam1, TResult>(
        scoped in TValue value,
        scoped in TParam1 param1)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TResult : allows ref struct;

    public delegate TResult OnFailureFunc<TError, TParam1, TResult>(
        scoped in TError error,
        scoped in TParam1 param1)
        where TError : IError, allows ref struct
        where TParam1 : allows ref struct
        where TResult : allows ref struct;

    public static TResult MatchWithDelegate<TValue, TError, TResult>(
        this scoped in ValueResult<TValue, TError> result,
        OnSuccessFunc<TValue, TResult> onSuccess,
        OnFailureFunc<TError, TResult> onFailure)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    public static TResult MatchWithDelegate<TValue, TError, TParam1, TResult>(
        this scoped in ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        OnSuccessFunc<TValue, TParam1, TResult> onSuccess,
        OnFailureFunc<TError, TParam1, TResult> onFailure)
        where TError : IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TResult : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, param1)
            : onFailure(result.Error, param1);

    public static ValueResult<TValue, ValueError> ErrorIfZero<TValue>(
        scoped in TValue value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : INumberBase<TValue>
        => TValue.IsZero(value)
            ? ValueResult.Failure<TValue>(
                $"{paramName} cannot be zero")
            : ValueResult.Success(value);

    public static ValueResult<TValue, ValueError> ErrorIfZero<TValue, TLogger>(
        scoped in TValue value,
        scoped in TLogger logger,
        Action<TLogger> logOnFailure,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : INumberBase<TValue>
        where TLogger : ILogger
        => TValue.IsZero(value)
            ? logger
                .LogAndReturn(logOnFailure)
                .Failure<TValue>(
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
                $"{paramName} cannot be greater than {other:##,#}")
            : ValueResult.Success(value);
}