// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Demo.Tools.Common.ValueResults;

public readonly ref struct TypedValueError(
    TypedValueError.ErrorTypes errorType,
    IError error)
    : IError
{
    public IError Error { get; } = error;
    public string Message => Error.Message;
    public ErrorTypes ErrorType { get; } = errorType;

    //public static TypedValueError General(string error)
    //    => new(ErrorTypes.General, error);

    //public static TypedValueError OutOfRange(string error)
    //    => new(ErrorTypes.OutOfRange, error);

    //public static TypedValueError InvalidOperation(string error)
    //    => new(ErrorTypes.InvalidOperation, error);

    //public static TypedValueError Unreachable(string error)
    //    => new(ErrorTypes.Unreachable, error);

    //public static ValueResult<TValue, TypedValueError> General<TValue>(string error)
    //    where TValue : allows ref struct
    //    => ValueResult.Failure<TValue, TypedValueError>(General(error));

    //public static ValueResult<TValue, TypedValueError> OutOfRange<TValue>(string error)
    //    where TValue : allows ref struct
    //    => ValueResult.Failure<TValue, TypedValueError>(OutOfRange(error));

    //public static ValueResult<TValue, TypedValueError> InvalidOperation<TValue>(string error)
    //    where TValue : allows ref struct
    //    => ValueResult.Failure<TValue, TypedValueError>(InvalidOperation(error));

    //public static ValueResult<TValue, TypedValueError> Unreachable<TValue>(string error)
    //    where TValue : allows ref struct
    //    => ValueResult.Failure<TValue, TypedValueError>(Unreachable(error));

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

    [TypedValueErrorGenerator<ErrorTypes>]
    public enum ErrorTypes
    {
        General,
        OutOfRange,
        InvalidOperation,

        Unreachable,
    }
}

public static class TypedValueErrorExtensions
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
}

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct)]
public class TypedValueErrorGeneratorAttribute<TEnum>
    : Attribute
    where TEnum : Enum
{

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

    public static TypedValueError<GeneralError> General(
        string message)
        => new(
            TypedValueError.ErrorTypes.General,
            new GeneralError(message));

    public static TypedValueError<ArgumentOutOfRangeError> OutOfRange(
        string? parameterName,
        string message)
        => new(
            TypedValueError.ErrorTypes.OutOfRange,
            new ArgumentOutOfRangeError(parameterName, message));

    public static TypedValueError<InvalidOperationError> InvalidOperation(
        string message)
        => new(
            TypedValueError.ErrorTypes.InvalidOperation,
            new InvalidOperationError(message));

    public static TypedValueError<UnreachableError> Unreachable(
        string message)
        => new(
            TypedValueError.ErrorTypes.Unreachable,
            new UnreachableError(message));
}

public readonly struct GeneralError(
    string message)
    : IError
{
    public string Message { get; } = message;

    [DoesNotReturn]
    public void ThrowAsException()
        => throw new Exception(Message);
}

public readonly struct ArgumentOutOfRangeError(
    string? parameterName,
    string message)
    : IError
{
    public string Message { get; } = message;
    public string? ParameterName { get; } = parameterName;

    [DoesNotReturn]
    public void ThrowAsException()
        => throw new ArgumentOutOfRangeException(ParameterName, Message);
}

public readonly struct InvalidOperationError(
    string message)
    : IError
{
    public string Message { get; } = message;

    [DoesNotReturn]
    public void ThrowAsException()
        => throw new InvalidOperationException(Message);
}

public readonly struct UnreachableError(
    string message)
    : IError
{
    public string Message { get; } = message;

    [DoesNotReturn]
    public void ThrowAsException()
        => throw new InvalidOperationException(Message);
}

public readonly ref struct LoggableError(
    string errorMessage,
    string messageTemplate,
    params object?[] arguments)
    : IError
{
    public string MessageTemplate { get; } = messageTemplate;

    public string Message { get; } = errorMessage;

    public void Log(ILogger logger, LogLevel logLevel)
        => logger.Log(logLevel, MessageTemplate, arguments);
}