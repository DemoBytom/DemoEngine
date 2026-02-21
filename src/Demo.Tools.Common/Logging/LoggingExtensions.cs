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
        => new(logger, nameof(Operation.Initialization));

    /// <summary>
    /// Logs information {ClassName} disposal {state} with state: "started" when context is
    /// created and "completed" when context is disposed.
    /// <para/>
    /// Can be used with using statement to log start and completion of class disposal
    /// within the using scope
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static LoggingContext<T> LogScopeDisposal<T>(
        this ILogger<T> logger)
        => new(logger, nameof(Operation.Disposal));

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "{ClassName} {Operation} {State}")]
    private static partial void LogClassOperationState(
        this ILogger logger,
        string className,
        string operation,
        string state);

    public readonly ref struct LoggingContext<T>
        : IDisposable
    {
        private readonly ILogger<T> _logger;
        private readonly string _className;
        private readonly string _operation;

        public LoggingContext(ILogger<T> logger,
            string operation,
            string? className = null)
        {
            _logger = logger;
            _operation = operation;
            _className = className ?? typeof(T).Name;
            _logger.LogClassOperationState(_className, operation: _operation, state: nameof(State.Started));
        }

        public void Dispose()
            => _logger.LogClassOperationState(_className, operation: _operation, state: nameof(State.Completed));
    }

    private enum State
    {
        Started,
        Completed,
    }

    private enum Operation
    {
        Initialization,
        Disposal,
    }
}