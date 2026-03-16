//HintName: LogAndReturnExtensions.g.cs
#nullable enable

namespace Demo.Tools.Common.ValueResults;

public static class LogAndReturnExtensions
{
    extension<TLogger>(TLogger? logger)
        where TLogger : global::Microsoft.Extensions.Logging.ILogger
    {
        /// <summary>
        /// LogAndReturn method with 0 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn(
            Action<TLogger> logAction)
        {
            if (logger is not null)
            {
                logAction(logger);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 1 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1>(
            Action<TLogger, TLogValue1> logAction,
            TLogValue1 value1)
            where TLogValue1 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 2 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2>(
            Action<TLogger, TLogValue1, TLogValue2> logAction,
            TLogValue1 value1,
            TLogValue2 value2)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 3 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2, TLogValue3>(
            Action<TLogger, TLogValue1, TLogValue2, TLogValue3> logAction,
            TLogValue1 value1,
            TLogValue2 value2,
            TLogValue3 value3)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
            where TLogValue3 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2, value3);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 4 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2, TLogValue3, TLogValue4>(
            Action<TLogger, TLogValue1, TLogValue2, TLogValue3, TLogValue4> logAction,
            TLogValue1 value1,
            TLogValue2 value2,
            TLogValue3 value3,
            TLogValue4 value4)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
            where TLogValue3 : allows ref struct
            where TLogValue4 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2, value3, value4);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 5 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5>(
            Action<TLogger, TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5> logAction,
            TLogValue1 value1,
            TLogValue2 value2,
            TLogValue3 value3,
            TLogValue4 value4,
            TLogValue5 value5)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
            where TLogValue3 : allows ref struct
            where TLogValue4 : allows ref struct
            where TLogValue5 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2, value3, value4, value5);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 6 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6>(
            Action<TLogger, TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6> logAction,
            TLogValue1 value1,
            TLogValue2 value2,
            TLogValue3 value3,
            TLogValue4 value4,
            TLogValue5 value5,
            TLogValue6 value6)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
            where TLogValue3 : allows ref struct
            where TLogValue4 : allows ref struct
            where TLogValue5 : allows ref struct
            where TLogValue6 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2, value3, value4, value5, value6);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 7 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6, TLogValue7>(
            Action<TLogger, TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6, TLogValue7> logAction,
            TLogValue1 value1,
            TLogValue2 value2,
            TLogValue3 value3,
            TLogValue4 value4,
            TLogValue5 value5,
            TLogValue6 value6,
            TLogValue7 value7)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
            where TLogValue3 : allows ref struct
            where TLogValue4 : allows ref struct
            where TLogValue5 : allows ref struct
            where TLogValue6 : allows ref struct
            where TLogValue7 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2, value3, value4, value5, value6, value7);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 8 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6, TLogValue7, TLogValue8>(
            Action<TLogger, TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6, TLogValue7, TLogValue8> logAction,
            TLogValue1 value1,
            TLogValue2 value2,
            TLogValue3 value3,
            TLogValue4 value4,
            TLogValue5 value5,
            TLogValue6 value6,
            TLogValue7 value7,
            TLogValue8 value8)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
            where TLogValue3 : allows ref struct
            where TLogValue4 : allows ref struct
            where TLogValue5 : allows ref struct
            where TLogValue6 : allows ref struct
            where TLogValue7 : allows ref struct
            where TLogValue8 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2, value3, value4, value5, value6, value7, value8);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 9 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6, TLogValue7, TLogValue8, TLogValue9>(
            Action<TLogger, TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6, TLogValue7, TLogValue8, TLogValue9> logAction,
            TLogValue1 value1,
            TLogValue2 value2,
            TLogValue3 value3,
            TLogValue4 value4,
            TLogValue5 value5,
            TLogValue6 value6,
            TLogValue7 value7,
            TLogValue8 value8,
            TLogValue9 value9)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
            where TLogValue3 : allows ref struct
            where TLogValue4 : allows ref struct
            where TLogValue5 : allows ref struct
            where TLogValue6 : allows ref struct
            where TLogValue7 : allows ref struct
            where TLogValue8 : allows ref struct
            where TLogValue9 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2, value3, value4, value5, value6, value7, value8, value9);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
        
        /// <summary>
        /// LogAndReturn method with 10 extra parameters
        /// </summary>
        public global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn<TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6, TLogValue7, TLogValue8, TLogValue9, TLogValue10>(
            Action<TLogger, TLogValue1, TLogValue2, TLogValue3, TLogValue4, TLogValue5, TLogValue6, TLogValue7, TLogValue8, TLogValue9, TLogValue10> logAction,
            TLogValue1 value1,
            TLogValue2 value2,
            TLogValue3 value3,
            TLogValue4 value4,
            TLogValue5 value5,
            TLogValue6 value6,
            TLogValue7 value7,
            TLogValue8 value8,
            TLogValue9 value9,
            TLogValue10 value10)
            where TLogValue1 : allows ref struct
            where TLogValue2 : allows ref struct
            where TLogValue3 : allows ref struct
            where TLogValue4 : allows ref struct
            where TLogValue5 : allows ref struct
            where TLogValue6 : allows ref struct
            where TLogValue7 : allows ref struct
            where TLogValue8 : allows ref struct
            where TLogValue9 : allows ref struct
            where TLogValue10 : allows ref struct
        {
            if (logger is not null)
            {
                logAction(logger, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10);
            }
            return new global::Demo.Tools.Common.ValueResults.LogAndReturnExtensions.LogAndReturnResultCallContext();
        }
    }
    
    public readonly ref struct LogAndReturnResultCallContext()
    {
    }
}
