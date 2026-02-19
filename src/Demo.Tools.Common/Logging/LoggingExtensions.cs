// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Logging;

namespace Demo.Tools.Common.Logging;

public static partial class LoggingExtensions
{
    /// <summary>
    /// Logs information {ClassName} initialization {state} with state: "started" when context is
    /// created and "completed" when context is disposed.
    /// <para/>
    /// Can be used with using statement to log start and completion of class initialization
    /// within the using scope - designed to work in constructors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static LoggingContext<T> LogScopeInitialization<T>(
        this ILogger<T> logger)
        => new(logger);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "{ClassName} initialization {State}")]
    internal static partial void LogClassInitializationState(
        this ILogger logger,
        string className,
        string state);
}

public readonly ref struct LoggingContext<T>
    : IDisposable
{
    private readonly ILogger<T> _logger;
    private readonly string _className;

    public LoggingContext(ILogger<T> logger)
    {
        _logger = logger;
        _className = typeof(T).Name;
        _logger.LogClassInitializationState(_className, state: "started");
    }

    public void Dispose()
        => _logger.LogClassInitializationState(_className, state: "completed");
}