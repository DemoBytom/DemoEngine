// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
using Demo.Engine.Benchmarks;
using Demo.Tools.Common.ValueResults;

namespace Demo.Engine.Benchmarks;

[MemoryDiagnoser]
[JitStatsDiagnoser]
[SimpleJob(runtimeMoniker: RuntimeMoniker.Net90, baseline: true)]
[SimpleJob(runtimeMoniker: RuntimeMoniker.Net10_0)]
public class ValueResultBenchmark
{
    public const int TEST_INT = 12345678;
    private const string PARAM_NAME = "TEST_INT";
    private const int MAX_VALUE = int.MaxValue - 1;

    public readonly record struct ReadonlyStruct(
        int Value1,
        int Value2,
        string Value3,
        string Value4);

    private readonly int
        _p1 = 1,
        _p2 = 2,
        _p3 = 3,
        _p4 = 4,
        _p5 = 5,
        _p6 = 6,
        _p7 = 7,
        _p8 = 8;

    [Benchmark]
    public ValueResult<int, ValueError> Test_Bind_Many_Parameters()
        => ValueResult
            .Success(TEST_INT)
            .Bind(
                param1: _p1,
                param2: _p2,
                param3: _p3,
                param4: _p4,
                param5: _p5,
                param6: _p6,
                param7: _p7,
                param8: _p8,
                bind: Bind_Many_Parameters_Func)
        ;

    [Benchmark]
    public ValueResult<int, ValueError> Test_Bind_Many_Parameters_InlineCall()
    => ValueResult
        .Success(TEST_INT)
        .Bind(
            param1: _p1,
            param2: _p2,
            param3: _p3,
            param4: _p4,
            param5: _p5,
            param6: _p6,
            param7: _p7,
            param8: _p8,
            bind: static (
                scoped in iValue,
                scoped in i1,
                scoped in i2,
                scoped in i3,
                scoped in i4,
                scoped in i5,
                scoped in i6,
                scoped in i7,
                scoped in i8)
            => ValueResult.Success(
                iValue + i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8))
    ;

    [Benchmark]
    public Result<int> Test_Class_Result_Many_Parameters()
        => Result
            .Success(TEST_INT)
            .Bind(value
                => Result.Success(
                    value
                    + _p1
                    + _p2
                    + _p3
                    + _p4
                    + _p5
                    + _p6
                    + _p7
                    + _p8))
        ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueResult<int, ValueError> Bind_Many_Parameters_Func(
        scoped in int iValue,
        scoped in int i1,
        scoped in int i2,
        scoped in int i3,
        scoped in int i4,
        scoped in int i5,
        scoped in int i6,
        scoped in int i7,
        scoped in int i8)
        => ValueResult.Success(
            iValue + i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8);

    public readonly struct BigStructHopefully
    {
        public readonly required int SomeInt { get; init; }
        public readonly required int SomeInt2 { get; init; }
        public readonly required string ThisIsAString { get; init; }
        public readonly required string ThisIsAnotherString { get; init; }
        public readonly required float SomeFloat { get; init; }
        public readonly required float SomeFloat2 { get; init; }
    }

    private readonly BigStructHopefully _testStruct = new()
    {
        SomeInt = 1,
        SomeInt2 = 2,
        ThisIsAString = "TEST",
        ThisIsAnotherString = "TEST",
        SomeFloat = 1.0f,
        SomeFloat2 = 2.0f
    };

    [Benchmark]
    public ValueResult<BigStructHopefully, ValueError> Test_Struct_ValueResult()
        => ValueResult
            .Success(_testStruct)
            .Bind(BindBigStructHopefullyFunc)
        ;

    [Benchmark]
    public ValueResult<BigStructHopefully, ValueError> Test_Struct_ValueResult_Inline()
    => ValueResult
        .Success(_testStruct)
        .Bind(static (
            scoped in value)
        => ValueResult.Success(value))
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueResult<BigStructHopefully, ValueError> BindBigStructHopefullyFunc(
        scoped in BigStructHopefully value)
        => ValueResult.Success(value);

    [Benchmark]
    public Result<BigStructHopefully> Test_Struct_Result()
        => Result
            .Success(_testStruct)
            .Bind(value
                => Result.Success(value))
        ;

    [Benchmark]
    public ValueResult<BigStructHopefully, ValueError> Test_Struct_ValueResult_Many_Parameters()
        => ValueResult
            .Success(_testStruct)
            .Bind(
                param1: _p1,
                param2: _p2,
                param3: _p3,
                param4: _p4,
                param5: _p5,
                param6: _p6,
                param7: _p7,
                param8: _p8,
                BindBigStructHopefullyFunc_Many_Parameters)
        ;

    [Benchmark]
    public ValueResult<BigStructHopefully, ValueError> Test_Struct_ValueResult_Many_Parameters_Inline()
        => ValueResult
            .Success(_testStruct)
            .Bind(
                param1: _p1,
                param2: _p2,
                param3: _p3,
                param4: _p4,
                param5: _p5,
                param6: _p6,
                param7: _p7,
                param8: _p8,
                static (
                    scoped in value,
                    scoped in i1,
                    scoped in i2,
                    scoped in i3,
                    scoped in i4,
                    scoped in i5,
                    scoped in i6,
                    scoped in i7,
                    scoped in i8)
            => ValueResult.Success(
                new BigStructHopefully()
                {
                    SomeInt = value.SomeInt
                                    + i1
                                    + i2
                                    + i3
                                    + i4
                                    + i5
                                    + i6
                                    + i7
                                    + i8,
                    SomeInt2 = value.SomeInt2
                                    + i1
                                    + i2
                                    + i3
                                    + i4
                                    + i5
                                    + i6
                                    + i7
                                    + i8,
                    ThisIsAString = value.ThisIsAnotherString,
                    ThisIsAnotherString = value.ThisIsAString,
                    SomeFloat = value.SomeFloat2,
                    SomeFloat2 = value.SomeFloat,
                }))
        ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueResult<BigStructHopefully, ValueError> BindBigStructHopefullyFunc_Many_Parameters(
        scoped in BigStructHopefully value,
        scoped in int i1,
        scoped in int i2,
        scoped in int i3,
        scoped in int i4,
        scoped in int i5,
        scoped in int i6,
        scoped in int i7,
        scoped in int i8)
        => ValueResult.Success(
            new BigStructHopefully()
            {
                SomeInt = value.SomeInt
                        + i1
                        + i2
                        + i3
                        + i4
                        + i5
                        + i6
                        + i7
                        + i8,
                SomeInt2 = value.SomeInt2
                        + i1
                        + i2
                        + i3
                        + i4
                        + i5
                        + i6
                        + i7
                        + i8,
                ThisIsAString = value.ThisIsAnotherString,
                ThisIsAnotherString = value.ThisIsAString,
                SomeFloat = value.SomeFloat2,
                SomeFloat2 = value.SomeFloat,
            });

    [Benchmark]
    public Result<BigStructHopefully> Test_Struct_Result_Many_Parameters()
        => Result
            .Success(_testStruct)
            .Bind(value
                => Result.Success(new BigStructHopefully()
                {
                    SomeInt = value.SomeInt
                        + _p1
                        + _p2
                        + _p3
                        + _p4
                        + _p5
                        + _p6
                        + _p7
                        + _p8,
                    SomeInt2 = value.SomeInt2
                        + _p1
                        + _p2
                        + _p3
                        + _p4
                        + _p5
                        + _p6
                        + _p7
                        + _p8,
                    ThisIsAString = value.ThisIsAnotherString,
                    ThisIsAnotherString = value.ThisIsAString,
                    SomeFloat = value.SomeFloat2,
                    SomeFloat2 = value.SomeFloat,
                }))
        ;

    [Benchmark]
    public ValueResult<int, ValueError> Test_Bind_Chaning_Types()
        => ValueResult
            .Success(_testStruct)
            .Bind(
                Test_Bind_Chaning_TypesFunc)
        ;

    private static ValueResult<int, ValueError> Test_Bind_Chaning_TypesFunc(
        scoped in BigStructHopefully value)
        => ValueResult.Success(
            value.SomeInt
            + value.SomeInt2
            + value.ThisIsAString.Length
            + value.ThisIsAnotherString.Length
            + (int)value.SomeFloat
            + (int)value.SomeFloat2);

    [Benchmark]
    public Result<int> Test_Class_Bind_Chaning_Types()
        => Result
            .Success(_testStruct)
            .Bind(value
                => Result.Success(
                    value.SomeInt
                    + value.SomeInt2
                    + value.ThisIsAString.Length
                    + value.ThisIsAnotherString.Length
                    + (int)value.SomeFloat
                    + (int)value.SomeFloat2))
        ;

    [Benchmark]
    public int TestValueResult_2_int()
    => ValueResultExtensions
        .ErrorIfZero(TEST_INT, PARAM_NAME)
        .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
        .Map(MapMethod)
        .MatchWithDelegate(
            OnSuccessMatch,
            OnFailureMatch);

    [Benchmark]
    public int TestResult_2_int()
        => Result
            .ErrorIfZero(TEST_INT, PARAM_NAME)
            .ErrorIfGreaterThen(MAX_VALUE, PARAM_NAME)
            .Map(static c => c)
            .Match(
                static i => i,
                static _ => throw new Exception(PARAM_NAME));

    private static int MapMethod(
        scoped in int value)
        => value;

    private static int OnSuccessMatch(
        scoped in int value)
        => value;

    [DoesNotReturn]
    private static int OnFailureMatch(
        scoped in ValueError error)
        => throw new Exception(PARAM_NAME);
}