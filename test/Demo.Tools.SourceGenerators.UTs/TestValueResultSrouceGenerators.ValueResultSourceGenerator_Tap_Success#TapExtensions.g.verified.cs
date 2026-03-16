//HintName: TapExtensions.g.cs
#nullable enable

namespace Demo.Tools.Common.ValueResults;

public static class TapExtensions
{
    extension<TValue, TError>(scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
    {
        /// <summary>
        /// <para>Tap extension method with 0 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap(
            TapAction<TValue> tap)
            {
                if (result.IsSuccess)
                {
                    tap(result.Value);
                }
                return result;
            }
        
        /// <summary>
        /// <para>Tap extension method with 1 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap<TParam1>(
            scoped in TParam1 param1,
            TapAction<TValue, TParam1> tap)
            where TParam1 : allows ref struct
            {
                if (result.IsSuccess)
                {
                    tap(result.Value, param1);
                }
                return result;
            }
        
        /// <summary>
        /// <para>Tap extension method with 2 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap<TParam1,TParam2>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            TapAction<TValue, TParam1, TParam2> tap)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            {
                if (result.IsSuccess)
                {
                    tap(result.Value, param1, param2);
                }
                return result;
            }
        
        /// <summary>
        /// <para>Tap extension method with 3 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap<TParam1,TParam2,TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            TapAction<TValue, TParam1, TParam2, TParam3> tap)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            {
                if (result.IsSuccess)
                {
                    tap(result.Value, param1, param2, param3);
                }
                return result;
            }
        
        /// <summary>
        /// <para>Tap extension method with 4 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap<TParam1,TParam2,TParam3,TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            TapAction<TValue, TParam1, TParam2, TParam3, TParam4> tap)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            {
                if (result.IsSuccess)
                {
                    tap(result.Value, param1, param2, param3, param4);
                }
                return result;
            }
        
        /// <summary>
        /// <para>Tap extension method with 5 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap<TParam1,TParam2,TParam3,TParam4,TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            TapAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> tap)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            where TParam5 : allows ref struct
            {
                if (result.IsSuccess)
                {
                    tap(result.Value, param1, param2, param3, param4, param5);
                }
                return result;
            }
        
        /// <summary>
        /// <para>Tap extension method with 6 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap<TParam1,TParam2,TParam3,TParam4,TParam5,TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            TapAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> tap)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            where TParam5 : allows ref struct
            where TParam6 : allows ref struct
            {
                if (result.IsSuccess)
                {
                    tap(result.Value, param1, param2, param3, param4, param5, param6);
                }
                return result;
            }
        
        /// <summary>
        /// <para>Tap extension method with 7 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap<TParam1,TParam2,TParam3,TParam4,TParam5,TParam6,TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            TapAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> tap)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            where TParam5 : allows ref struct
            where TParam6 : allows ref struct
            where TParam7 : allows ref struct
            {
                if (result.IsSuccess)
                {
                    tap(result.Value, param1, param2, param3, param4, param5, param6, param7);
                }
                return result;
            }
        
        /// <summary>
        /// <para>Tap extension method with 8 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (T → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Tap<TParam1,TParam2,TParam3,TParam4,TParam5,TParam6,TParam7,TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            TapAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> tap)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            where TParam5 : allows ref struct
            where TParam6 : allows ref struct
            where TParam7 : allows ref struct
            where TParam8 : allows ref struct
            {
                if (result.IsSuccess)
                {
                    tap(result.Value, param1, param2, param3, param4, param5, param6, param7, param8);
                }
                return result;
            }
        
        
        /// <summary>
        /// <para>TapError extension method with 0 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError(
            TapErrorAction<TError> tapError)
            {
                if (result.IsError)
                {
                    tapError(result.Error);
                }
                return result;
            }
        
        /// <summary>
        /// <para>TapError extension method with 1 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError<TParam1>(
            scoped in TParam1 param1,
            TapErrorAction<TError, TParam1> tapError)
            where TParam1 : allows ref struct
            {
                if (result.IsError)
                {
                    tapError(result.Error, param1);
                }
                return result;
            }
        
        /// <summary>
        /// <para>TapError extension method with 2 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError<TParam1,TParam2>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            TapErrorAction<TError, TParam1, TParam2> tapError)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            {
                if (result.IsError)
                {
                    tapError(result.Error, param1, param2);
                }
                return result;
            }
        
        /// <summary>
        /// <para>TapError extension method with 3 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError<TParam1,TParam2,TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            TapErrorAction<TError, TParam1, TParam2, TParam3> tapError)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            {
                if (result.IsError)
                {
                    tapError(result.Error, param1, param2, param3);
                }
                return result;
            }
        
        /// <summary>
        /// <para>TapError extension method with 4 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError<TParam1,TParam2,TParam3,TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4> tapError)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            {
                if (result.IsError)
                {
                    tapError(result.Error, param1, param2, param3, param4);
                }
                return result;
            }
        
        /// <summary>
        /// <para>TapError extension method with 5 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError<TParam1,TParam2,TParam3,TParam4,TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5> tapError)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            where TParam5 : allows ref struct
            {
                if (result.IsError)
                {
                    tapError(result.Error, param1, param2, param3, param4, param5);
                }
                return result;
            }
        
        /// <summary>
        /// <para>TapError extension method with 6 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError<TParam1,TParam2,TParam3,TParam4,TParam5,TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> tapError)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            where TParam5 : allows ref struct
            where TParam6 : allows ref struct
            {
                if (result.IsError)
                {
                    tapError(result.Error, param1, param2, param3, param4, param5, param6);
                }
                return result;
            }
        
        /// <summary>
        /// <para>TapError extension method with 7 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError<TParam1,TParam2,TParam3,TParam4,TParam5,TParam6,TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> tapError)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            where TParam5 : allows ref struct
            where TParam6 : allows ref struct
            where TParam7 : allows ref struct
            {
                if (result.IsError)
                {
                    tapError(result.Error, param1, param2, param3, param4, param5, param6, param7);
                }
                return result;
            }
        
        /// <summary>
        /// <para>TapError extension method with 8 extra parameters.</para>
        /// <para>Runs side-effects without changing the result.</para>
        /// </summary>
        /// <remarks>
        /// ValueResult&lt;T, E&gt; → (E → void) → ValueResult&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> TapError<TParam1,TParam2,TParam3,TParam4,TParam5,TParam6,TParam7,TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> tapError)
            where TParam1 : allows ref struct
            where TParam2 : allows ref struct
            where TParam3 : allows ref struct
            where TParam4 : allows ref struct
            where TParam5 : allows ref struct
            where TParam6 : allows ref struct
            where TParam7 : allows ref struct
            where TParam8 : allows ref struct
            {
                if (result.IsError)
                {
                    tapError(result.Error, param1, param2, param3, param4, param5, param6, param7, param8);
                }
                return result;
            }
        
    }
    
    /// <summary>
    /// Tap delegate with 0 extra parameters
    /// </summary>
    public delegate void TapAction<TValue>(
        scoped in TValue value)
        where TValue : allows ref struct;
    
    /// <summary>
    /// Tap delegate with 1 extra parameters
    /// </summary>
    public delegate void TapAction<TValue, TParam1>(
        scoped in TValue value,
        scoped in TParam1 param1)
        where TValue : allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// Tap delegate with 2 extra parameters
    /// </summary>
    public delegate void TapAction<TValue, TParam1, TParam2>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// Tap delegate with 3 extra parameters
    /// </summary>
    public delegate void TapAction<TValue, TParam1, TParam2, TParam3>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// Tap delegate with 4 extra parameters
    /// </summary>
    public delegate void TapAction<TValue, TParam1, TParam2, TParam3, TParam4>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct;
    
    /// <summary>
    /// Tap delegate with 5 extra parameters
    /// </summary>
    public delegate void TapAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct;
    
    /// <summary>
    /// Tap delegate with 6 extra parameters
    /// </summary>
    public delegate void TapAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct;
    
    /// <summary>
    /// Tap delegate with 7 extra parameters
    /// </summary>
    public delegate void TapAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct;
    
    /// <summary>
    /// Tap delegate with 8 extra parameters
    /// </summary>
    public delegate void TapAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        scoped in TParam8 param8)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        where TParam8 : allows ref struct;
    
    /// <summary>
    /// TapError delegate with 0 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError>(
        scoped in TError error)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct;
    
    /// <summary>
    /// TapError delegate with 1 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError, TParam1>(
        scoped in TError error,
        scoped in TParam1 param1)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// TapError delegate with 2 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError, TParam1, TParam2>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// TapError delegate with 3 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError, TParam1, TParam2, TParam3>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// TapError delegate with 4 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct;
    
    /// <summary>
    /// TapError delegate with 5 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct;
    
    /// <summary>
    /// TapError delegate with 6 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct;
    
    /// <summary>
    /// TapError delegate with 7 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct;
    
    /// <summary>
    /// TapError delegate with 8 extra parameters
    /// </summary>
    public delegate void TapErrorAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        scoped in TParam8 param8)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        where TParam8 : allows ref struct;
}
