// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Demo.Tools.Common.ValueResults;

[StructLayout(LayoutKind.Auto)]
public readonly ref struct ValueResult<TValue, TError>
    : IResult<TValue, TError>
    where TError : IError, allows ref struct
    where TValue : allows ref struct
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError => !IsSuccess;

    public TValue? Value
    {
        get
        {
            Debug.Assert(IsSuccess);
            return field;
        }
    }

    public TError? Error
    {
        get
        {
            Debug.Assert(IsError);
            return field;
        }
    }

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

public readonly ref struct ValueResult<TError>
    : IResult<TError>
    where TError : IError, allows ref struct
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError => !IsSuccess;

    public TError? Error { get; }

    private ValueResult(
        bool isSuccess,
        scoped in TError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static ValueResult<TError> Success()
        => new(
            isSuccess: true,
            error: default);

    public static ValueResult<TError> Failure(
        scoped in TError error)
        => new(
            isSuccess: false,
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

    public static ValueResult<TValue, ValueError> Failure<TValue>(
        ValueError error)
        where TValue : allows ref struct
        => ValueResult<TValue, ValueError>.Failure(error);

    public static ValueResult<ValueError> Success()
        => ValueResult<ValueError>.Success();

    public static ValueResult<ValueError> Failure(
        scoped in ValueError error)
        => ValueResult<ValueError>.Failure(error);

    public static ValueResult<ValueError> Failure(
        scoped in string error)
        => ValueResult<ValueError>.Failure(new(error));

    public static ValueResult<TError> Success<TError>()
        where TError : IError, allows ref struct
        => ValueResult<TError>.Success();

    public static ValueResult<TError> Failure<TError>(
        scoped in TError error)
        where TError : IError, allows ref struct
        => ValueResult<TError>.Failure(error);

    extension(scoped in LogAndReturnExtensions.LogAndReturnResultCallContext _)
    {
#pragma warning disable CA1822 // Mark members as static
        public ValueResult<TValue, ValueError> Failure<TValue>(
            string error)
            where TValue : allows ref struct
            => ValueResult<TValue, ValueError>.Failure(new(error));

        public ValueResult<TValue, ValueError> Success<TValue>(
            TValue value)
            where TValue : allows ref struct
            => ValueResult<TValue, ValueError>.Success(value);

        public ValueResult<TValue, TypedValueError> InvalidOperation<TValue>(
            string errorMessage)
            where TValue : allows ref struct
            => TypedValueError.InvalidOperation<TValue>(errorMessage);

        public ValueResult<TValue, TypedValueError> OutOfRange<TValue>(
            string parameterName,
            string errorMessage)
            where TValue : allows ref struct
            => TypedValueError.OutOfRange<TValue>(parameterName, errorMessage);
#pragma warning restore CA1822 // Mark members as static
    }
}