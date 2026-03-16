// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using BenchmarkDotNet.Attributes;
using Demo.Tools.Common.ValueResults;
using static Demo.Tools.Common.ValueResults.TapExtensions;
using static Demo.Tools.Common.ValueResults.ValueResultExtensions;

namespace Demo.Engine.Benchmarks;

[MemoryDiagnoser]
//[LongRunJob]
public class TapBenchmarks
{
    public int TestParam { get; private set; } = 10;

    [Benchmark(Baseline = true)]
    public ValueResult<int, ValueError> TapBenchmark()
    {
        var result = ValueResult
            .Success(42)
            .Tap((scoped in val) => TestParam = val)
            .Match(
                onSuccess: static (scoped in val) => val,
                onFailure: static (scoped in err) => -1)
            ;

        return ValueResult.Success(result + TestParam);
    }

    [Benchmark]
    public ValueResult<int, ValueError> TapBenchmark_Static_Call()
    {
        var result = ValueResult
            .Success(42)
            .Tap(
                param1: this,
                tap: static (scoped in val, scoped in @this) => @this.TestParam = val)
            .Match(
                onSuccess: static (scoped in val) => val,
                onFailure: static (scoped in err) => -1)
            ;

        return ValueResult.Success(result + TestParam);
    }
}