//HintName: MatchExtensions.g.cs
#nullable enable

namespace Demo.Tools.Common.ValueResults;

public static class MatchExtensions
{
    
    /// <summary>
    /// Match extension method with 0 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        OnSuccessFunc<TValue, TResult> onSuccess,
        OnFailureFunc<TError, TResult> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    
    /// <summary>
    /// Match extension method with 1 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult, TParam1>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        OnSuccessFunc<TValue, TResult, TParam1> onSuccess,
        OnFailureFunc<TError, TResult, TParam1> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, in param1)
            : onFailure(result.Error, in param1);
    
    /// <summary>
    /// Match extension method with 2 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult, TParam1, TParam2>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        OnSuccessFunc<TValue, TResult, TParam1, TParam2> onSuccess,
        OnFailureFunc<TError, TResult, TParam1, TParam2> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, in param1, in param2)
            : onFailure(result.Error, in param1, in param2);
    
    /// <summary>
    /// Match extension method with 3 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult, TParam1, TParam2, TParam3>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3> onSuccess,
        OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, in param1, in param2, in param3)
            : onFailure(result.Error, in param1, in param2, in param3);
    
    /// <summary>
    /// Match extension method with 4 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult, TParam1, TParam2, TParam3, TParam4>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4> onSuccess,
        OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, in param1, in param2, in param3, in param4)
            : onFailure(result.Error, in param1, in param2, in param3, in param4);
    
    /// <summary>
    /// Match extension method with 5 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4, TParam5> onSuccess,
        OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, in param1, in param2, in param3, in param4, in param5)
            : onFailure(result.Error, in param1, in param2, in param3, in param4, in param5);
    
    /// <summary>
    /// Match extension method with 6 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onSuccess,
        OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, in param1, in param2, in param3, in param4, in param5, in param6)
            : onFailure(result.Error, in param1, in param2, in param3, in param4, in param5, in param6);
    
    /// <summary>
    /// Match extension method with 7 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onSuccess,
        OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, in param1, in param2, in param3, in param4, in param5, in param6, in param7)
            : onFailure(result.Error, in param1, in param2, in param3, in param4, in param5, in param6, in param7);
    
    /// <summary>
    /// Match extension method with 8 extra parameters
    /// </summary>
    public static TResult Match<TValue, TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        scoped in TParam8 param8,
        OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onSuccess,
        OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        where TParam8 : allows ref struct
        => result.IsSuccess
            ? onSuccess(result.Value, in param1, in param2, in param3, in param4, in param5, in param6, in param7, in param8)
            : onFailure(result.Error, in param1, in param2, in param3, in param4, in param5, in param6, in param7, in param8);
    
    
    /// <summary>
    /// Match extension method with 0 extra parameters
    /// </summary>
    public static void Match<TValue, TError>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        OnSuccessAction<TValue> onSuccess,
        OnFailureAction<TError> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value);
        }
        else
        {
            onFailure(result.Error);
        }
    }
    
    /// <summary>
    /// Match extension method with 1 extra parameters
    /// </summary>
    public static void Match<TValue, TError, TParam1>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        OnSuccessAction<TValue, TParam1> onSuccess,
        OnFailureAction<TError, TParam1> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value, in param1);
        }
        else
        {
            onFailure(result.Error, in param1);
        }
    }
    
    /// <summary>
    /// Match extension method with 2 extra parameters
    /// </summary>
    public static void Match<TValue, TError, TParam1, TParam2>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        OnSuccessAction<TValue, TParam1, TParam2> onSuccess,
        OnFailureAction<TError, TParam1, TParam2> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value, in param1, in param2);
        }
        else
        {
            onFailure(result.Error, in param1, in param2);
        }
    }
    
    /// <summary>
    /// Match extension method with 3 extra parameters
    /// </summary>
    public static void Match<TValue, TError, TParam1, TParam2, TParam3>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        OnSuccessAction<TValue, TParam1, TParam2, TParam3> onSuccess,
        OnFailureAction<TError, TParam1, TParam2, TParam3> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value, in param1, in param2, in param3);
        }
        else
        {
            onFailure(result.Error, in param1, in param2, in param3);
        }
    }
    
    /// <summary>
    /// Match extension method with 4 extra parameters
    /// </summary>
    public static void Match<TValue, TError, TParam1, TParam2, TParam3, TParam4>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4> onSuccess,
        OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value, in param1, in param2, in param3, in param4);
        }
        else
        {
            onFailure(result.Error, in param1, in param2, in param3, in param4);
        }
    }
    
    /// <summary>
    /// Match extension method with 5 extra parameters
    /// </summary>
    public static void Match<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5> onSuccess,
        OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value, in param1, in param2, in param3, in param4, in param5);
        }
        else
        {
            onFailure(result.Error, in param1, in param2, in param3, in param4, in param5);
        }
    }
    
    /// <summary>
    /// Match extension method with 6 extra parameters
    /// </summary>
    public static void Match<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onSuccess,
        OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value, in param1, in param2, in param3, in param4, in param5, in param6);
        }
        else
        {
            onFailure(result.Error, in param1, in param2, in param3, in param4, in param5, in param6);
        }
    }
    
    /// <summary>
    /// Match extension method with 7 extra parameters
    /// </summary>
    public static void Match<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onSuccess,
        OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value, in param1, in param2, in param3, in param4, in param5, in param6, in param7);
        }
        else
        {
            onFailure(result.Error, in param1, in param2, in param3, in param4, in param5, in param6, in param7);
        }
    }
    
    /// <summary>
    /// Match extension method with 8 extra parameters
    /// </summary>
    public static void Match<TValue, TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        this scoped in global::Demo.Tools.Common.ValueResults.ValueResult<TValue, TError> result,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7,
        scoped in TParam8 param8,
        OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onSuccess,
        OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> onFailure)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        where TParam8 : allows ref struct
    {
        if(result.IsSuccess)
        {
            onSuccess(result.Value, in param1, in param2, in param3, in param4, in param5, in param6, in param7, in param8);
        }
        else
        {
            onFailure(result.Error, in param1, in param2, in param3, in param4, in param5, in param6, in param7, in param8);
        }
    }
    
    /// <summary>
    /// Match OnSuccessFunc delegate with 0 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult>(
        scoped in TValue value)
        where TValue : allows ref struct
        where TResult : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 0 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult>(
        scoped in TError error)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TResult : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 0 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue>(
        scoped in TValue value)
        where TValue : allows ref struct;
    
    /// <summary>
    /// Match OnFailureAction delegate with 0 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError>(
        scoped in TError error)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct;
    
    /// <summary>
    /// Match OnSuccessFunc delegate with 1 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult, TParam1>(
        scoped in TValue value,
        scoped in TParam1 param1)
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 1 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult, TParam1>(
        scoped in TError error,
        scoped in TParam1 param1)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 1 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue, TParam1>(
        scoped in TValue value,
        scoped in TParam1 param1)
        where TValue : allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureAction delegate with 1 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError, TParam1>(
        scoped in TError error,
        scoped in TParam1 param1)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessFunc delegate with 2 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult, TParam1, TParam2>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 2 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult, TParam1, TParam2>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 2 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue, TParam1, TParam2>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureAction delegate with 2 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError, TParam1, TParam2>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessFunc delegate with 3 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 3 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 3 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue, TParam1, TParam2, TParam3>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TValue : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureAction delegate with 3 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError, TParam1, TParam2, TParam3>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessFunc delegate with 4 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4)
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 4 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 4 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4>(
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
    /// Match OnFailureAction delegate with 4 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4>(
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
    /// Match OnSuccessFunc delegate with 5 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4, TParam5>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5)
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 5 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 5 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5>(
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
    /// Match OnFailureAction delegate with 5 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5>(
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
    /// Match OnSuccessFunc delegate with 6 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6)
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 6 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 6 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
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
    /// Match OnFailureAction delegate with 6 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
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
    /// Match OnSuccessFunc delegate with 7 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        scoped in TValue value,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7)
        where TValue : allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 7 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        scoped in TError error,
        scoped in TParam1 param1,
        scoped in TParam2 param2,
        scoped in TParam3 param3,
        scoped in TParam4 param4,
        scoped in TParam5 param5,
        scoped in TParam6 param6,
        scoped in TParam7 param7)
        where TError : global::Demo.Tools.Common.ValueResults.IError, allows ref struct
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 7 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
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
    /// Match OnFailureAction delegate with 7 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
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
    /// Match OnSuccessFunc delegate with 8 extra parameters
    /// </summary>
    public delegate TResult OnSuccessFunc<TValue, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
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
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        where TParam8 : allows ref struct;
    
    /// <summary>
    /// Match OnFailureFunc delegate with 8 extra parameters
    /// </summary>
    public delegate TResult OnFailureFunc<TError, TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
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
        where TResult : allows ref struct
        where TParam1 : allows ref struct
        where TParam2 : allows ref struct
        where TParam3 : allows ref struct
        where TParam4 : allows ref struct
        where TParam5 : allows ref struct
        where TParam6 : allows ref struct
        where TParam7 : allows ref struct
        where TParam8 : allows ref struct;
    
    /// <summary>
    /// Match OnSuccessAction delegate with 8 extra parameters
    /// </summary>
    public delegate void OnSuccessAction<TValue, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
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
    /// Match OnFailureAction delegate with 8 extra parameters
    /// </summary>
    public delegate void OnFailureAction<TError, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
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
