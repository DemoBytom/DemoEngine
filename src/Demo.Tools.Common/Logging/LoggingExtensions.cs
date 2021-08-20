using System;
using Microsoft.Extensions.Logging;

namespace Demo.Tools.Common.Logging
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Logs information {class} initialization {state} with state: "started" when context is
        /// created and "completed" when context is disposed.
        /// <para/>
        /// Can be used with using statement to log start and completion of class initialization
        /// within the using scope - designed to work in constructors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static LoggingContext<T> LogScopeInitialization<T>(this ILogger<T> logger) => new(logger);
    }

    public class LoggingContext<T> : IDisposable
    {
        private bool _disposedValue = false;
        private readonly ILogger<T> _logger;
        private readonly string _className;

        public LoggingContext(ILogger<T> logger)
        {
            _logger = logger;
            _className = typeof(T).Name;
            _logger.LogDebug("{class} initialization {state}", _className, "started");
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.LogDebug("{class} initialization {state}", _className, "completed");
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}