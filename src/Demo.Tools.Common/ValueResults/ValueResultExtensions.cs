// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Demo.Tools.Common.ValueResults;

public static class ValueResultExtensions
{
    public static ValueResult<TValue, ValueError> ErrorIfGreaterThen<TValue>(
        this scoped in ValueResult<TValue, ValueError> value,
        scoped in TValue other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : IComparable<TValue>
        => value.Bind(
            other,
            paramName,
            ValueResult.ErrorIfGreaterThen);

    extension<TValue, TError>(scoped in ValueResult<TValue, TError> result)
        where TValue : allows ref struct
        where TError : IError, allows ref struct
    {

        public ValueResult<TValue, TNewError> MapError<TNewError>(
            Func<TError, TNewError> map)
            where TNewError : IError, allows ref struct
            => result.IsSuccess
                ? ValueResult<TValue, TNewError>.Success(result.Value)
                : ValueResult<TValue, TNewError>.Failure(map(result.Error));

        public void MatchFailure(
            Action<TError> onFailure)
        {
            if (result.IsSuccess)
            {
                return;
            }

            onFailure(result.Error);
        }
    }

    extension(ValueResult)
    {
        public static ValueResult<TValue, ValueError> ErrorIfZero<TValue>(
            scoped in TValue val,
            [CallerArgumentExpression(nameof(val))] string? paramName = null)
            where TValue : INumberBase<TValue>
            => TValue.IsZero(val)
                ? ValueResult.Failure<TValue>(
                    $"{paramName} cannot be zero")
                : ValueResult.Success(val);

        public static ValueResult<TValue, ValueError> ErrorIfZero<TValue, TLogger>(
            scoped in TValue val, // parameter named `value` breaks if used as a named parameter: https://github.com/dotnet/roslyn/issues/81251 Fixed in Roslyn 18.3 
            scoped in TLogger logger,
            Action<TLogger> logOnFailure,
            [CallerArgumentExpression(nameof(val))] string? paramName = null)
            where TValue : INumberBase<TValue>
            where TLogger : ILogger
            => TValue.IsZero(val)
                ? logger
                    .LogAndReturn(logOnFailure)
                    .Failure<TValue>(
                        $"{paramName} cannot be zero")
                : ValueResult.Success(val);

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
}