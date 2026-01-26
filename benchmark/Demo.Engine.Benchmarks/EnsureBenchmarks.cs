// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using BenchmarkDotNet.Attributes;
using Demo.Tools.Common.ValueResults;

namespace Demo.Engine.Benchmarks;

[MemoryDiagnoser]
public class EnsureBenchmarks
{
    public static IEnumerable<object[]> ParamsSource()
    {
        yield return new object[] { (10, 5) };
        yield return new object[] { (5, 10) };
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(ParamsSource))]
    public ValueResult<(int i, int t), TypedValueError> ValidateUsingBind_StringInterpolation(
        (int index, int treshold) @params)
        => ValueResult<(int index, int treshold), TypedValueError>
            .Success(@params)
            .Bind(static (scoped in p)
                => p.index >= p.treshold
                ? ValueResult<(int i, int t), TypedValueError>.Success(p)
                : TypedValueError.OutOfRange<(int index, int treshold)>(
                    parameterName: nameof(p.index),
                    error: $"Index {p.index} is below treshold {p.treshold}"));

    [Benchmark]
    [ArgumentsSource(nameof(ParamsSource))]
    public ValueResult<(int i, int t), TypedValueError> ValidateUsingBind_StaticString(
        (int index, int treshold) @params)
        => ValueResult<(int index, int treshold), TypedValueError>
            .Success(@params)
            .Bind(static (scoped in p)
                => p.index >= p.treshold
                ? ValueResult<(int i, int t), TypedValueError>.Success(p)
                : TypedValueError.OutOfRange<(int index, int treshold)>(
                    parameterName: nameof(p.index),
                    error: "Index {p.index} is below treshold {p.treshold}"));

    [Benchmark]
    [ArgumentsSource(nameof(ParamsSource))]
    public ValueResult<(int i, int t), TypedValueError> ValidateUsingEnsure_StringInterpolation(
        (int index, int treshold) @params)
        => ValueResult<(int index, int treshold), TypedValueError>
            .Success(@params)
            .Ensure(
                static (scoped in p)
                    => p.index >= p.treshold,
                static (scoped in p)
                    => new(
                        TypedValueError.ErrorTypes.OutOfRange,
                        new ArgumentOutOfRangeError(
                            parameterName: nameof(p.index),
                            message: $"Index {p.index} is below treshold {p.treshold}")));

    [Benchmark]
    [ArgumentsSource(nameof(ParamsSource))]
    public ValueResult<(int i, int t), TypedValueError> ValidateUsingEnsure_StaticString(
        (int index, int treshold) @params)
        => ValueResult<(int index, int treshold), TypedValueError>
            .Success(@params)
            .Ensure(
                static (scoped in p)
                    => p.index >= p.treshold,
                static (scoped in p)
                    => new(
                        TypedValueError.ErrorTypes.OutOfRange,
                        new ArgumentOutOfRangeError(
                            parameterName: nameof(p.index),
                            message: "Index {p.index} is below treshold {p.treshold}")));
}