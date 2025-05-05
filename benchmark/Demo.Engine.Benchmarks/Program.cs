// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Demo.Tools.Common;

BenchmarkRunner.Run<ValueResultBenchmark>();

[MemoryDiagnoser]
public class ValueResultBenchmark
{
    public const int TEST_INT = 12345678;
    private const string PARAM_NAME = "TEST_INT";
    private const int MAX_VALUE = int.MaxValue - 1;

    [Benchmark]
    public int TestValueResult_int()
        => ValueResultExtensions
            .ErrorIfZero(TEST_INT, PARAM_NAME)
            .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
            .Map(MapMethod)
            .Map(MapMethod2)
            .Map(MapMethod3)
            .Map(MapMethod4)
            .Match(
                success => success,
                _ => throw new Exception(PARAM_NAME));

    [Benchmark]
    public ValueResult<int, ValueError> TestValueResult_int_ValueResult()
        => ValueResultExtensions
            .ErrorIfZero(TEST_INT, PARAM_NAME)
            .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
            .Map(MapMethod)
            .Map(MapMethod2)
            .Map(MapMethod3)
            .Map(MapMethod4);

    [Benchmark]
    public int TestResult_int()
      => Result
          .ErrorIfZero(TEST_INT, PARAM_NAME)
          .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
          .Map(v => v)
          .Map(v => v + 10)
          .Map(v => v * 100)
          .Map(v => v / 100)
          .Match(
              success => success,
              _ => throw new Exception(PARAM_NAME));

    [Benchmark]
    public Result<int> TestResult_int_Result()
      => Result
          .ErrorIfZero(TEST_INT, PARAM_NAME)
          .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
          .Map(v => v)
          .Map(v => v + 10)
          .Map(v => v * 100)
          .Map(v => v / 100)
          ;

    //[Benchmark]
    //public int TestValueResult_2_int()
    //=> ValueResultExtensions
    //    .ErrorIfZero(TEST_INT, PARAM_NAME)
    //    .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
    //    .Map(MapMethod)
    //    .MatchWithDelegate(
    //        OnSuccessMatch,
    //        OnFailureMatch);

    //[Benchmark]
    //public int TestValueResult_3_int()
    //    => ValueResult.Success(TEST_INT)
    //        .Value;

    //[Benchmark]
    //public int TestValueResult_4_int()
    //    => ValueResult.Success(TEST_INT)
    //        .Map(MapMethod)
    //        .Value;

    //[Benchmark]
    //public int TestValueResult_5_int()
    //    => ValueResult.Success(TEST_INT)
    //        .Match(
    //            value => value,
    //            _ => throw new Exception(PARAM_NAME));

    //[Benchmark]
    //public int TestValueResult_6_int()
    //    => ValueResult.Success(TEST_INT)
    //        .MatchWithDelegate(
    //            OnSuccessMatch,
    //            OnFailureMatch);

    //[Benchmark]
    //public int TestValueResult_7_int()
    //    => ValueResultExtensions
    //        .ErrorIfZero(TEST_INT, PARAM_NAME)
    //        .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
    //        .Value;

    //[Benchmark]
    //public int TestValueResult_8_int()
    //    => ValueResult.Success(TEST_INT)
    //        .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
    //        .Value;

    //[Benchmark]
    //public int TestValueResult_9_int()
    //   => ValueResultExtensions
    //       .ErrorIfGreaterThen(TEST_INT, MAX_VALUE, PARAM_NAME)
    //       .Value;

    //[Benchmark]
    //public int TestValueResult_10_int()
    //   => ValueResult.Success(TEST_INT)
    //        .Bind(BindMethod)
    //       .Value;

    private static ValueResult<int, ValueError> BindMethod(
        scoped in int value)
        => ValueResult<int, ValueError>.Success(value);

    private static int MapMethod(
        scoped in int value)
        => value;

    private static int MapMethod2(
        scoped in int value)
        => value + 10;

    private static int MapMethod3(
        scoped in int value)
        => value * 100;

    private static int MapMethod4(
        scoped in int value)
        => value / 100;

    private static int OnSuccessMatch(
        scoped in int value)
        => value;

    [DoesNotReturn]
    private static int OnFailureMatch(
        scoped in ValueError error)
        => throw new Exception(PARAM_NAME);
}

public class Error(
    string error)
{
    public string Message { get; } = error;
}

public class Result<TValue>
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public TValue? Value { get; }
    public Error? Error { get; }

    private Result(
        bool isSuccess, TValue? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TValue> Failure(
        string error) => new(
            isSuccess: false,
            value: default,
            error: new Error(error));

    public static Result<TValue> Success(
        TValue value)
        => new(
            isSuccess: true,
            value: value,
            error: null);
}

public static class Result
{
    public static Result<TValue> Success<TValue>(
        TValue value)
        => Result<TValue>.Success(value);

    public static Result<TValue> Failure<TValue>(
        string error)
        => Result<TValue>.Failure(
            error);

    public static Result<TValue> Map<TValue>(
        this Result<TValue> result,
        Func<TValue, TValue> map)
        => result.IsSuccess
            ? Result<TValue>.Success(map(result.Value))
            : Result<TValue>.Failure(result.Error.Message);

    public static Result<TValue2> Bind<TValue1, TValue2>(
        this Result<TValue1> result,
        Func<TValue1, Result<TValue2>> bind)
        => result.IsSuccess
            ? bind(result.Value)
            : Result<TValue2>.Failure(result.Error.Message);

    public static TResult Match<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    public static Result<TValue> ErrorIfZero<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : INumberBase<TValue>
        => TValue.IsZero(value)
            ? Result<TValue>.Failure(
                $"{paramName} cannot be zero")
            : Result<TValue>.Success(value);

    public static Result<TValue> ErrorIfGreaterThen<TValue>(
        this Result<TValue> value,
        TValue other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : IComparable<TValue>
        => value
            .Bind(v
                => ErrorIfGreaterThen(
                    value: v,
                    other: other,
                    paramName: paramName));

    public static Result<TValue> ErrorIfGreaterThen<TValue>(
        TValue value,
        TValue other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TValue : IComparable<TValue>
        => value.CompareTo(other) > 0
            ? Result<TValue>.Failure(
                $"{paramName} cannot be greater than {other}")
            : Result<TValue>.Success(value);
}