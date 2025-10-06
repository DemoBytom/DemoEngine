// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Demo.Tools.Common.ValueResults;

public readonly ref struct TypedValueError(
    TypedValueError.ErrorTypes errorType,
    IError error)
    : IError
{
    public IError InnerError { get; } = error;
    public string Message => InnerError.Message;
    public ErrorTypes ErrorType { get; } = errorType;

    public static ValueResult<TValue, TypedValueError> General<TValue>(string error)
        where TValue : allows ref struct
        => ValueResult.Failure<TValue, TypedValueError>(
            new(
                ErrorTypes.General,
                new GeneralError(error)));

    public static ValueResult<TValue, TypedValueError> OutOfRange<TValue>(
        string parameterName,
        string error)
        where TValue : allows ref struct
        => ValueResult.Failure<TValue, TypedValueError>(
            new(
                ErrorTypes.OutOfRange,
                new ArgumentOutOfRangeError(parameterName, error)));

    public static ValueResult<TValue, TypedValueError> InvalidOperation<TValue>(string error)
        where TValue : allows ref struct
        => ValueResult.Failure<TValue, TypedValueError>(
            new(
                ErrorTypes.InvalidOperation,
                new InvalidOperationError(error)));

    public static ValueResult<TValue, TypedValueError> Unreachable<TValue>(string error)
        where TValue : allows ref struct
        => ValueResult.Failure<TValue, TypedValueError>(
            new(
                ErrorTypes.Unreachable,
                new UnreachableError(error)));

    public enum ErrorTypes
    {
        General,
        OutOfRange,
        InvalidOperation,

        Unreachable,
    }
}

public static partial class TypedValueErrorExtensions
{
    public static ValueResult<TValue, TypedValueError> LogAndReturnOutOfRange<TValue>(
        this ILogger? logger,
        Action<ILogger> logAction,
        string parameterName,
        string errorMessage)
        where TValue : allows ref struct
    {
        if (logger is not null)
        {
            logAction(logger);
        }
        return TypedValueError.OutOfRange<TValue>(parameterName, errorMessage);
    }

    public static ValueResult<TValue, TypedValueError> LogAndReturnOutOfRange<TValue, TLogValue1, TLogValue2>(
        this ILogger? logger,
        (Action<ILogger, TLogValue1, TLogValue2> logAction, TLogValue1 logVal1, TLogValue2 logVal2) logAction,
        string parameterName,
        string errorMessage)
        where TValue : allows ref struct
    {
        if (logger is not null)
        {
            logAction.logAction(logger, logAction.logVal1, logAction.logVal2);
        }
        return TypedValueError.OutOfRange<TValue>(parameterName, errorMessage);
    }

    public static ValueResult<TValue, TypedValueError> LogAndReturnInvalidOperation<TValue>(
        this ILogger? logger,
        Action<ILogger> logAction,
        string errorMessage)
        where TValue : allows ref struct
    {
        if (logger is not null)
        {
            logAction(logger);
        }

        return TypedValueError.InvalidOperation<TValue>(errorMessage);
    }

    public static ValueResult<TValue, TypedValueError> InvalidOperation<TValue>(
        this ValueResult.LogAndReturnResultCallContext _,
        string errorMessage)
        where TValue : allows ref struct
        => TypedValueError.InvalidOperation<TValue>(errorMessage);

    public static ValueResult<TValue, TypedValueError> OutOfRange<TValue>(
        this ValueResult.LogAndReturnResultCallContext _,
        string parameterName,
        string errorMessage)
        where TValue : allows ref struct
        => TypedValueError.OutOfRange<TValue>(parameterName, errorMessage);
}

public readonly ref struct TypedValueError<TError>(
    TypedValueError.ErrorTypes errorType,
    TError error)
    : IError
    where TError : IError, allows ref struct
{
    public TypedValueError.ErrorTypes ErrorType { get; } = errorType;

    public string Message => Error.Message;

    public TError Error { get; } = error;
}

public readonly struct GeneralError(
    string message)
    : IThrowableError
{
    public string Message { get; } = message;

    [DoesNotReturn]
    public void ThrowAsException()
        => throw new Exception(Message);

    [DoesNotReturn]
    public TReturn ThrowAsException<TReturn>()
        where TReturn : allows ref struct
        => throw new Exception(Message);
}

public readonly struct ArgumentOutOfRangeError(
    string? parameterName,
    string message)
    : IThrowableError
{
    public string Message { get; } = message;
    public string? ParameterName { get; } = parameterName;

    [DoesNotReturn]
    public void ThrowAsException()
        => throw new ArgumentOutOfRangeException(ParameterName, Message);

    [DoesNotReturn]
    public TReturn ThrowAsException<TReturn>()
        where TReturn : allows ref struct
        => throw new ArgumentOutOfRangeException(Message);
}

public readonly struct InvalidOperationError(
    string message)
    : IThrowableError
{
    public string Message { get; } = message;

    [DoesNotReturn]
    public void ThrowAsException()
        => throw new InvalidOperationException(Message);

    [DoesNotReturn]
    public TReturn ThrowAsException<TReturn>()
        where TReturn : allows ref struct
        => throw new InvalidOperationException(Message);
}

public readonly struct UnreachableError(
    string message)
    : IThrowableError
{
    public string Message { get; } = message;

    [DoesNotReturn]
    public void ThrowAsException()
        => throw new UnreachableException(Message);

    [DoesNotReturn]
    public TReturn ThrowAsException<TReturn>()
        where TReturn : allows ref struct
        => throw new UnreachableException(Message);
}

public interface IThrowableError
    : IError
{
    [DoesNotReturn]
    void ThrowAsException();

    [DoesNotReturn]
    TReturn ThrowAsException<TReturn>()
        where TReturn : allows ref struct;
}