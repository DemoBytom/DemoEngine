// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.NetStandard.Win32.WindowMessage
{
    internal enum PowerBroadcastValues : uint
    {
        /// <summary>
        /// PBT_APMQUERYSUSPEND: Requests permission to suspend the computer. An application that
        /// grants permission should carry out preparations for the suspension before returning.
        /// </summary>
        QuerySuspend = 0,

        /// <summary>
        /// Notifies applications that the system has resumed operation after being suspended.
        /// </summary>
        ResumeSuspend = 7
    }
}