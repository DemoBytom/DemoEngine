// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;

namespace Demo.Engine.Core.Interfaces.Platform
{
    public interface IOSMessageHandler
    {
        /// <summary>
        /// Call this on each tick of the main loop to process all Windows messages in the queue
        /// </summary>
        /// <returns>if the Tick is successful</returns>
        /// <exception cref="InvalidOperationException">An error occured</exception>
        bool DoEvents(IRenderingControl control);
    }
}