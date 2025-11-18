// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Error = Demo.Engine.Benchmarks.Result.Error;

namespace Demo.Engine.Benchmarks;

public sealed class Result<TValue>
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public TValue? Value { get; }
    public Error? Error { get; }

    private Result(
        bool isSuccess, TValue? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TValue> Failure(
        string error) => new(
            isSuccess: false,
            value: default,
            error: new Error(error));

    public static Result<TValue> Success(
        TValue value)
        => new(
            isSuccess: true,
            value: value,
            error: null);
}

public static class Result
{
    public sealed class Error(
        string error)
    {
        public string Message { get; } = error;
    }

    public static Result<TValue> Success<TValue>(
        TValue value)
        => Result<TValue>.Success(value);

    public static Result<TValue> Failure<TValue>(
        string error)
        => Result<TValue>.Failure(
            error);

    public static Result<TValue> Map<TValue>(
        this Result<TValue> result,
        Func<TValue, TValue> map)
        => result.IsSuccess
            ? Result<TValue>.Success(map(result.Value))
            : Result<TValue>.Failure(result.Error.Message);

    public static Result<TValue2> Bind<TValue1, TValue2>(
        this Result<TValue1> result,
        Func<TValue1, Result<TValue2>> bind)
        => result.IsSuccess
            ? bind(result.Value)
            : Result<TValue2>.Failure(result.Error.Message);

    public static TResult Match<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    public static Result<TValue> ErrorIfZero<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : INumberBase<TValue>
        => TValue.IsZero(value)
            ? Result<TValue>.Failure(
                $"{paramName} cannot be zero")
            : Result<TValue>.Success(value);

    public static Result<TValue> ErrorIfGreaterThen<TValue>(
        this Result<TValue> value,
        TValue other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : IComparable<TValue>
        => value
            .Bind(v
                => ErrorIfGreaterThen(
                    value: v,
                    other: other,
                    paramName: paramName));

    public static Result<TValue> ErrorIfGreaterThen<TValue>(
        TValue value,
        TValue other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : IComparable<TValue>
        => value.CompareTo(other) > 0
            ? Result<TValue>.Failure(
                $"{paramName} cannot be greater than {other}")
            : Result<TValue>.Success(value);
}