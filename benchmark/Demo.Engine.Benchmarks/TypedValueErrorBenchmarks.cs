// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
using Demo.Tools.Common.ValueResults;
using static Demo.Tools.Common.ValueResults.TypedValueError;

namespace Demo.Engine.Benchmarks;

[MemoryDiagnoser]
[JitStatsDiagnoser]
[SimpleJob(runtimeMoniker: RuntimeMoniker.Net90, baseline: true)]
[SimpleJob(runtimeMoniker: RuntimeMoniker.Net10_0)]
public class TypedValueErrorBenchmarks
{
    [Benchmark(Baseline = true)]
    public ValueResult<int, TypedValueError> OutOfRangeError()
    {
        var error = new TypedValueError(
                ErrorTypes.OutOfRange,
                new ArgumentOutOfRangeError(
                    "PARAM_NAME",
                    "Error Message"));

        return ValueResult<int, TypedValueError>.Failure(error);
    }

    [Benchmark]
    public ValueResult<int, TypedValueError<ArgumentOutOfRangeError>> OutOfRangeError_Generic()
    {
        var error = TypedValueError<ArgumentOutOfRangeError>.OutOfRange(
            "PARAM_NAME",
            "Error Message");
        return ValueResult<int, TypedValueError<ArgumentOutOfRangeError>>.Failure(error);
    }

    [Benchmark]
    public ValueResult<int, LoggableError> LoggableError()
    {
        var error = new LoggableError(
            $"Error occured: {1}",
            "Error occured: {someInt}",
            [1]);

        return ValueResult<int, LoggableError>.Failure(error);
    }
}