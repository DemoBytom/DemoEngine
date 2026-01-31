//HintName: EnsureExtensions.g.cs
#nullable enable

namespace Demo.Tools.Common.ValueResults;

public static class EnsureExtensions
{
    extension<TValue, TError>(scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
    {
        
        /// <summary>
        /// <para>Ensure extension method with 0 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure(
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 1 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1>(
            scoped in TParam1 param1,
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 2 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 3 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
        
        /// <summary>
        /// <para>Ensure extension method with 1 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1>(
            scoped in TParam1 param1,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 1 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1>(
            scoped in TParam1 param1,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 2 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 3 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value, param1)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
        
        /// <summary>
        /// <para>Ensure extension method with 2 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 2 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 2 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 3 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value, param1, param2)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
        
        /// <summary>
        /// <para>Ensure extension method with 3 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 3 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 3 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 3 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 4 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 5 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 6 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 7 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7));
        
        /// <summary>
        /// <para>Ensure extension method with 8 extra parameters</para>
        /// <para>Validate a successful value.</para>
        /// </summary>
        /// <remarks>
        /// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;
        /// </remarks>
        public global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> Ensure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
            scoped in TParam1 param1,
            scoped in TParam2 param2,
            scoped in TParam3 param3,
            scoped in TParam4 param4,
            scoped in TParam5 param5,
            scoped in TParam6 param6,
            scoped in TParam7 param7,
            scoped in TParam8 param8,
            EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> predicate,
            EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onError)
            => result.IsError
            || predicate(result.Value, param1, param2, param3, param4, param5, param6, param7, param8)
                ? result
                : global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError>.Failure(
                    onError(result.Value, param1, param2, param3, param4, param5, param6, param7, param8));
    }
    
    /// <summary>
    /// Ensure predicate delegate with 0 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue>(
        scoped in TValue value)
        where TValue : allows ref struct;
    
    /// <summary>
    /// Ensure predicate delegate with 1 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue, TParam1>(
        scoped in TValue value,
        scoped in TParam1 param1)
        where TValue : allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// Ensure predicate delegate with 2 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue, TParam1, TParam2>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// Ensure predicate delegate with 3 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue, TParam1, TParam2, TParam3>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// Ensure predicate delegate with 4 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4>(
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
    /// Ensure predicate delegate with 5 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5>(
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
    /// Ensure predicate delegate with 6 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
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
    /// Ensure predicate delegate with 7 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
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
    /// Ensure predicate delegate with 8 extra parameters
    /// </summary>
    public delegate bool EnsurePredicate<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
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
    /// Ensure on error func delegate with 0 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError>(
        scoped in TValue value)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct;
    
    /// <summary>
    /// Ensure on error func delegate with 1 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError, TParam1>(
        scoped in TValue value,
        scoped in TParam1 param1)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// Ensure on error func delegate with 2 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError, TParam1, TParam2>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// Ensure on error func delegate with 3 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// Ensure on error func delegate with 4 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct;
    
    /// <summary>
    /// Ensure on error func delegate with 5 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct;
    
    /// <summary>
    /// Ensure on error func delegate with 6 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct;
    
    /// <summary>
    /// Ensure on error func delegate with 7 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7)
        where TValue : allows ref struct
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct;
    
    /// <summary>
    /// Ensure on error func delegate with 8 extra parameters
    /// </summary>
    public delegate TError EnsureOnErrorFunc<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
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
