// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Tools.Common.ValueResults;

public static class BindExtensions
{
    /// <summary>
    /// Bind function without extra parameters
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError>(
       scoped in TValue1 value)
       where TError : IError, allows ref struct
       where TValue1 : allows ref struct
       where TValue2 : allows ref struct;

    /// <summary>
    /// Bind function with one extra parameter
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1>(
        scoped in TValue1 value,
        scoped in TParam1 param1)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct;

    /// <summary>
    /// Bind function with two extra parameters
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;

    /// <summary>
    /// Bind function with three extra parameters
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;

    /// <summary>
    /// Bind function with four extra parameters
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct;

    /// <summary>
    /// Bind function with five extra parameters
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct;

    /// <summary>
    /// Bind function with six extra parameters
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct;

    /// <summary>
    /// Bind function with seven extra parameters
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct;

    /// <summary>
    /// Bind function with eight extra parameters
    /// </summary>
    public delegate ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        scoped in TParam8 param8)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        where TParam8 : allows ref struct;

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError>(
        this scoped in ValueResult<TValue1, TError> result,
        BindFunc<TValue1, TValue2, TError> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1>(
        this scoped in ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        BindFunc<TValue1, TValue2, TError, TParam1> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2>(
        this scoped in ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3>(
        this scoped in ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4>(
        this scoped in ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3, in param4)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5>(
        this scoped in ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3, in param4, in param5)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        this scoped in ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3, in param4, in param5, in param6)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        this scoped in ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3, in param4, in param5, in param6, in param7)
            : ValueResult<TValue2, TError>.Failure(result.Error);

    public static ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        this scoped in ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        scoped in TParam8 param8,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> bind)
        where TError : IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        where TParam8 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3, in param4, in param5, in param6, in param7, in param8)
            : ValueResult<TValue2, TError>.Failure(result.Error);
}