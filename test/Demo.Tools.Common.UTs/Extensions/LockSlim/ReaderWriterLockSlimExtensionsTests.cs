// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Text;
using Demo.Tools.Common.Extensions.LockSlim;
using Shouldly;
using Xunit;

namespace Demo.Tools.Common.UTs.Extensions.LockSlim;

public class ReaderWriterLockSlimExtensionsTests
{
    [Fact]
    public async Task Test_Lock_2()
    {
        //T1 - EnterWriteLock
        //T1 - Start writing
        //T2 - EnterWriteLock - wait
        //T1 - End writing
        //T1 - ExitWriteLock
        //T2 - EnterWriteLock
        //T2 - Start writing
        //T2 - End writing
        //T2 - ExitWriteLock

        const string T1 = "THREAD_1";
        const string T2 = "THREAD_2";

        using var lockSlim = new ReaderWriterLockSlim();
        var sb = new StringBuilder();
        var threadJobs = new List<ThreadJob>
            {
                new(T1, () => lockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(20))),
                new(T1, () =>
                {
                    _ = sb.Append("T1 start");
                    Thread.Sleep(5);
                }),
                new(T2, () => lockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(20))),
                new(T1, () =>
                {
                    _ = sb.Append("T1 end");
                    Thread.Sleep(5);
                }),
                new(T1, lockSlim.ExitWriteLock),
                new(T2, () =>
                {
                    _ = sb.Append("T2 start");
                    Thread.Sleep(5);
                }),
                new(T2, () =>
                {
                    _ = sb.Append("T2 end");
                    Thread.Sleep(5);
                }),
                new(T2, lockSlim.ExitWriteLock)
            };

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        var tw1 = new ThreadWorker(T1, threadJobs, cts.Token);
        var tw2 = new ThreadWorker(T2, threadJobs, cts.Token);

        await Task.WhenAll(
        new[] {
                tw1.Start(),
                tw2.Start()
        });

        sb.ToString().ShouldContain("T1 startT1 end");
        sb.ToString().ShouldContain("T2 startT2 end");
    }

    [Fact]
    public async Task Test_Write_Lock_Extensions()
    {
        using var lockSlim = new ReaderWriterLockSlim();

        const string T1 = "THREAD_1";
        const string T2 = "THREAD_2";
        const string T3 = "T1_BREAKER_THREAD";

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        var t1Started = false;
        var t2Started = false;
        var t1FinishedWriting = false;
        var sb = new StringBuilder();

        var threadJobs = new List<ThreadJob>
            {
                new(T1, () => lockSlim.EnterWriteLockBlock(() =>
                {
                    _ = sb.Append("T1 start");
                    while (!cts.IsCancellationRequested && !t1FinishedWriting)
                    {
                        Thread.Sleep(5);
                        t1Started = true;
                    }
                    _ = sb.Append("T1 end");
                })),
                new(T2, () =>
                {
                    while (!cts.IsCancellationRequested && !t1Started)
                    {
                        Thread.Sleep(5);
                    }
                    _ = sb.Append("T2 attempt");
                    t2Started = true;
                    _ = lockSlim.EnterWriteLockBlock(() =>
                    {
                        _ = sb.Append("T2 start");
                        Thread.Sleep(5);
                        _ = sb.Append("T2 end");
                    });
                }),
                new(T3, () =>
                {
                    while (!t1Started || !t2Started)
                    {
                        t1FinishedWriting = false;
                    }

                    t1FinishedWriting = true;
                })
            };

        var tw1 = new ThreadWorker(T1, threadJobs, cts.Token);
        var tw2 = new ThreadWorker(T2, threadJobs, cts.Token);
        var tw3 = new ThreadWorker(T3, threadJobs, cts.Token);

        await Task.WhenAll(
            tw1.Start(),
            tw2.Start(),
            tw3.Start());

        sb.ToString().ShouldBe(
            "T1 start" +
            "T2 attempt" +
            "T1 end" +
            "T2 start" +
            "T2 end");
    }
}