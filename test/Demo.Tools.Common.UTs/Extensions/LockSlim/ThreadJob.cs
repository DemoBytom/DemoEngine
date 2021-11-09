// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;

namespace Demo.Tools.Common.UTs.Extensions.LockSlim
{
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

        private void Attempt() =>
            Attempted = Attempted
                ? throw new Exception($"{ThreadId} ALREADY ATTEMPTED!")
                : true;
    }
}