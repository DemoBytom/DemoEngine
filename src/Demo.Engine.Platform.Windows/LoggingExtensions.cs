// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.Windows;

internal static partial class LoggingExtensions
{
    private const string ERROR_IN_MESSAGE_LOOP_PROCESSING_WINDOWS_MESSAGES = "An error occured in message loop while processing windows messages. Error: {ErrorCode}";
    private const string EXCEPTION_IN_MESSAGE_LOOP_PROCESSING_WINDOWS_MESSAGES = "An exception occured in message loop while processing windows messages. Exception message: {ExceptionMessage}";
    private const string EXCEPTION_DISPOSING_WINDOWS_MESSAGE_PUMP = "An exception occured while disposing WindowsMessagePump.";

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = ERROR_IN_MESSAGE_LOOP_PROCESSING_WINDOWS_MESSAGES,
        SkipEnabledCheck = true)]
    private static partial void LogErroInMainLoopProcessingWindowsMessages(
        this ILogger<WindowsMessagePump> logger,
        int errorCode);

    internal static void LogErroInMainLoopProcessingWindowsMessages(
        this ILogger<WindowsMessagePump> logger,
        Func<int> errorCodeAction)
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogErroInMainLoopProcessingWindowsMessages(
                errorCodeAction());
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = EXCEPTION_IN_MESSAGE_LOOP_PROCESSING_WINDOWS_MESSAGES,
        SkipEnabledCheck = true)]
    private static partial void LogExceptionInMessageLoopProcessingWindowsMessages(
        this ILogger<WindowsMessagePump> logger,
        Exception exception,
        string exceptionMessage);

    internal static void LogExceptionInMessageLoopProcessingWindowsMessages(
        this ILogger<WindowsMessagePump> logger,
        Exception exception)
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogExceptionInMessageLoopProcessingWindowsMessages(
                exception,
                exception.Message);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = EXCEPTION_DISPOSING_WINDOWS_MESSAGE_PUMP,
        SkipEnabledCheck = true)]
    public static partial void LogExceptionDisposingWindowsMessagePump(
        this ILogger<WindowsMessagePump> logger,
        Exception exception);

}