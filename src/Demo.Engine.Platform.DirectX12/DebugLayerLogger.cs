// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces;

namespace Demo.Engine.Platform.DirectX12;

public class DebugLayerLogger : IDebugLayerLogger
{
    public void LogMessages()
    {
    }

    public ulong MessageQueuePosition()
        => 0;

    public DebugLayerMessage[] ReadMessages(
        ulong readFrom = 0)
        => Array.Empty<DebugLayerMessage>();

    public T WrapCallInMessageExceptionHandler<T>(
        Func<T> func)
        => func();

    public void WrapCallInMessageExceptionHandler(
        Action act)
        => act();
}