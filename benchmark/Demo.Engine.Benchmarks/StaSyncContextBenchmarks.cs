// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Demo.Engine.Core.Features.StaThread;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using WorkItem = Demo.Engine.Core.Features.StaThread.StaThreadService.StaSingleThreadedSynchronizationContext.WorkItem;

namespace Demo.Engine.Benchmarks;

[MemoryDiagnoser]
[HideColumns("Mean", "Error", "StdDev", "Median", "Ratio", "RadioSD")]
public class StaSyncContextBenchmarks
{
    private readonly Random _rng = new(1050);
    private ObjectPool<WorkItem>? _workItemPool;
    private ServiceProvider? _serviceProvider = null!;

    [Params(10_000, 100_000)]
    public int IterationCount { get; set; }

    [GlobalSetup]
    public void SetupObjectPool()
    {
        _serviceProvider = new ServiceCollection()
            .AddStaWorkItemObjectPool()
            .BuildServiceProvider();

        _workItemPool = _serviceProvider.GetRequiredService<ObjectPool<WorkItem>>();
    }

    [GlobalCleanup]
    public void CleanupObjectPool()
    {
        _serviceProvider?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public async Task<long> StaSyncContextWorkNoPool()
    {
        StaThreadService.StaSingleThreadedSynchronizationContext? syncContext = null;

        try
        {
            syncContext = new StaThreadService.StaSingleThreadedSynchronizationContext();

            SynchronizationContext.SetSynchronizationContext(syncContext);
            await Task.Yield();

            long returnValue = 0;
            // Act
            for (var i = 0; i < IterationCount; i++)
            {
                returnValue += await AddValues(returnValue, _rng.Next(1, 100))
                    .ConfigureAwait(true);
            }

            return returnValue;
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(null);
            await Task.Yield();

            syncContext?.Dispose();
        }
    }

    [Benchmark]
    public async Task<long> StaSyncContextWork()
    {
        StaThreadService.StaSingleThreadedSynchronizationContext? syncContext = null;

        try
        {
            syncContext = new StaThreadService.StaSingleThreadedSynchronizationContext(
                _workItemPool);

            SynchronizationContext.SetSynchronizationContext(syncContext);
            await Task.Yield();

            long returnValue = 0;
            // Act
            for (var i = 0; i < IterationCount; i++)
            {
                returnValue += await AddValues(returnValue, _rng.Next(1, 100))
                    .ConfigureAwait(true);
                //returnValue += _rng.Next(1, 100);
            }

            return returnValue;
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(null);
            await Task.Yield();

            syncContext?.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async ValueTask<long> AddValues(long a, int b)
    {
        await Task.Yield();
        a += b;
        await Task.Yield();

        return a;
    }
}