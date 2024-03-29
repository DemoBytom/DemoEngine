// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Tools.Common.UTs.Extensions.LockSlim;

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