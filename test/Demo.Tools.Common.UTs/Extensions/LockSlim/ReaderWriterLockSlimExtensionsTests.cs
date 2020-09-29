using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

//using Demo.Tools.Common.Extensions.LockSlim;

namespace Demo.Tools.Common.UTs.Extensions.LockSlim
{
    public class ReaderWriterLockSlimExtensionsTests
    {
        [Fact]
        public async Task Test_Lock()
        {
            using var lockSlim = new ReaderWriterLockSlim();
            for (var i = 0; i < 10; ++i)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var sb = new StringBuilder();

                var taskCompletionSource1 = new TaskCompletionSource<bool>();
                var taskCompletionSource2 = new TaskCompletionSource<bool>();

                var t1 = new Thread(() =>
                {
                    lockSlim.EnterWriteLock();
                    try
                    {
                        try
                        {
                            sb.AppendLine("T1 start");
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                            sb.AppendLine("T1 end");
                            taskCompletionSource1.SetResult(true);
                        }
                        finally
                        {
                            lockSlim.ExitWriteLock();
                        }
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource1.SetException(ex);
                    }
                });

                var t2 = new Thread(() =>
                {
                    lockSlim.EnterWriteLock();
                    try
                    {
                        try
                        {
                            sb.AppendLine("T2 start");
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                            sb.AppendLine("T2 end");
                            taskCompletionSource2.SetResult(true);
                        }
                        finally
                        {
                            lockSlim.ExitWriteLock();
                        }
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource2.SetException(ex);
                    }
                });

                t1.Start();
                t2.Start();

                await Task.WhenAll(
                    taskCompletionSource1.Task,
                    taskCompletionSource2.Task);

                sb.ToString().Should().Contain(
                    @"T1 start
T1 end",
                    $"Failed at attempt no {i}");
                sb.ToString().Should().Contain(
                @"T2 start
T2 end",
                $"Failed at attempt no {i}");
            }
        }

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
            var threadJobs = new List<ThreadJob>();
            threadJobs.Add(new ThreadJob(T1, () => lockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(20))));
            threadJobs.Add(new ThreadJob(T1, () =>
            {
                sb.Append("T1 start");
                Thread.Sleep(5);
            }));
            threadJobs.Add(new ThreadJob(T2, () => lockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(20))));
            threadJobs.Add(new ThreadJob(T1, () =>
            {
                sb.Append("T1 end");
                Thread.Sleep(5);
            }));
            threadJobs.Add(new ThreadJob(T1, () => lockSlim.ExitWriteLock()));
            threadJobs.Add(new ThreadJob(T2, () =>
            {
                sb.Append("T2 start");
                Thread.Sleep(5);
            }));
            threadJobs.Add(new ThreadJob(T2, () =>
            {
                sb.Append("T2 end");
                Thread.Sleep(5);
            }));
            threadJobs.Add(new ThreadJob(T2, () => lockSlim.ExitWriteLock()));

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            var tw1 = new ThreadWorker(T1, threadJobs, cts.Token);
            var tw2 = new ThreadWorker(T2, threadJobs, cts.Token);

            await Task.WhenAll(
                tw1.Start(),
                tw2.Start());

            sb.ToString().Should().Contain("T1 startT1 end");
            sb.ToString().Should().Contain("T2 startT2 end");
        }
    }

    public class ThreadWorker
    {
        private readonly Thread _thread;
        private readonly TaskCompletionSource<bool> _completionSource;

        public ThreadWorker(
            string threadID,
            List<ThreadJob> threadJobs,
            CancellationToken cts)
        {
            _completionSource = new TaskCompletionSource<bool>();
            _thread = new Thread(() =>
            {
                try
                {
                    var i = 0;
                    while (i < threadJobs.Count)
                    {
                        if (cts.IsCancellationRequested)
                        {
                            throw new TaskCanceledException(threadID);
                        }

                        var tj = threadJobs[i];

                        if (tj.ThreadId == threadID)
                        {
                            tj.Perform();
                            i++;
                        }
                        else if (tj.Attempted)
                        {
                            i++;
                        }
                    }
                    _completionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    _completionSource.SetException(ex);
                }
            });
        }

        public Task Start()
        {
            _thread.Start();
            return _completionSource.Task;
        }
    }

    public class ThreadJob
    {
        public ThreadJob(
            string threadId,
            Action act)
        {
            ThreadId = threadId;
            Act = act;
            Completed = false;
        }

        public string ThreadId { get; }
        public Action Act { get; }
        public bool Completed { get; private set; }
        public bool Attempted { get; private set; }

        public void Perform()
        {
            if (!Completed)
            {
                Attempt();
                Act();
                Completed = true;
            }
            else
            {
                throw new Exception($"{ThreadId} ALREADY COMPLETED!");
            }
        }

        private void Attempt()
        {
            if (!Attempted)
            {
                Attempted = true;
            }
            else
            {
                throw new Exception($"{ThreadId} ALREADY ATTEMPTED!");
            }
        }
    }
}