//HintName: BindExtensions.g.cs
#nullable enable

namespace Demo.Tools.Common.ValueResults;

public static class BindExtensions
{
    /// <summary>
    /// Bind delegate with 0 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError>(
        scoped in TValue1 value)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct;
    
    /// <summary>
    /// Bind delegate with 1 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1>(
        scoped in TValue1 value,
        scoped in TParam1 param1)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// Bind delegate with 2 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// Bind delegate with 3 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// Bind delegate with 4 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct;
    
    /// <summary>
    /// Bind delegate with 5 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct;
    
    /// <summary>
    /// Bind delegate with 6 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct;
    
    /// <summary>
    /// Bind delegate with 7 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
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
    /// Bind delegate with 8 extra parameters
    /// </summary>
    public delegate global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        scoped in TValue1 value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        scoped in TParam8 param8)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
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
    
    /// <summary>
    /// Bind extension method with 0 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        BindFunc<TValue1, TValue2, TError> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value)
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
    
    /// <summary>
    /// Bind extension method with 1 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        BindFunc<TValue1, TValue2, TError, TParam1> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1)
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
    
    /// <summary>
    /// Bind extension method with 2 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2)
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
    
    /// <summary>
    /// Bind extension method with 3 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3)
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
    
    /// <summary>
    /// Bind extension method with 4 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3, in param4)
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
    
    /// <summary>
    /// Bind extension method with 5 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue1 : allows ref struct
        where TValue2 : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        => result.IsSuccess
            ? bind(result.Value, in param1, in param2, in param3, in param4, in param5)
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
    
    /// <summary>
    /// Bind extension method with 6 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
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
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
    
    /// <summary>
    /// Bind extension method with 7 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
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
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
    
    /// <summary>
    /// Bind extension method with 8 extra parameters
    /// </summary>
    public static global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue1, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        scoped in TParam8 param8,
        BindFunc<TValue1, TValue2, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> bind)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
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
            : global::Demo.Tools.Common.ValueResults.ValueResult<TValue2, TError>.Failure(result.Error);
}
