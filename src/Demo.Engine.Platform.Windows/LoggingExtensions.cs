// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.Windows;

internal static partial class LoggingExtensions
{
    private const string ERROR_IN_MAIN_LOOP_PROCESSING_WINDOWS_MESSAGES = "An error occured in main loop while processing windows messages. Error: {ErrorCode}";

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = ERROR_IN_MAIN_LOOP_PROCESSING_WINDOWS_MESSAGES,
        SkipEnabledCheck = true)]
    private static partial void LogErroInMainLoopProcessingWindowsMessages(
        this ILogger<WindowsMessagesHandler> logger,
        int errorCode);

    internal static void LogErroInMainLoopProcessingWindowsMessages(
        this ILogger<WindowsMessagesHandler> logger,
        Func<int> errorCodeAction)
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogErroInMainLoopProcessingWindowsMessages(
                errorCodeAction());
        }
    }
}