// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

// TODO: Add to DI via KeyedServices
internal sealed class RenderingCommand
    : IDisposable
{
    public ID3D12CommandQueue CommandQueue { get; }

    public ID3D12GraphicsCommandList10 CommandList { get; }

    public uint FrameIndex { get; private set; } = 0;

    private readonly CommandFrame[] _commandFrames;

    private readonly ID3D12Fence1 _fence;
    private ulong _fenceValue = 0;
    private readonly AutoResetEvent _fenceEvent;
    private readonly ILogger<RenderingCommand> _logger;

    public RenderingCommand(
        ILogger<RenderingCommand> logger,
        ID3D12Device14 device,
        CommandListType commandListType)
    {
        _logger = logger;

        CommandQueue = device.CreateCommandQueue(
            type: commandListType,
            priority: CommandQueuePriority.Normal,
            flags: CommandQueueFlags.None,
            nodeMask: 0);

        CommandQueue.NameObject(
            name: commandListType switch
            {
                CommandListType.Direct
                    => "GFX Command Queue",
                CommandListType.Compute
                    => "Compute Command Queue",
                _
                    => "Command Queue",
            },
            logger: _logger);

        CommandList = device.CreateCommandList1<ID3D12GraphicsCommandList10>(
            type: commandListType,
            commandListFlags: CommandListFlags.None);
        CommandList.NameObject(
            name: commandListType switch
            {
                CommandListType.Direct
                    => "GFX Command List",
                CommandListType.Compute
                    => "Compute Command List",
                _
                    => "Command List",
            },
            logger: _logger);

        _commandFrames = new CommandFrame[Common.FRAME_BUFFER_COUNT];
        for (var i = 0; i < _commandFrames.Length; ++i)
        {
            var allocator = device.CreateCommandAllocator(
                commandListType);
            allocator.NameObject(
                commandListType switch
                {
                    CommandListType.Direct
                        => $"GFX Command Allocator[{i}]",
                    CommandListType.Compute
                        => $"Compute Command Allocator[{i}]",
                    _
                        => $"Command Allocator[{i}]",
                },
                _logger);

            _commandFrames[i] = new CommandFrame(logger, allocator);
        }

        _fence = device.CreateFence<ID3D12Fence1>(
            _fenceValue,
            FenceFlags.None);
        _fence.NameObject(
            "My fence!",
            _logger);

        _fenceEvent = new AutoResetEvent(false);
    }

    public void BeginFrame()
    {
        var commandFrame = _commandFrames[FrameIndex];

        commandFrame.Wait(
            _fenceEvent,
            _fence);

        var commandList = CommandList;
        commandFrame.Reset(
            in commandList);
    }

    public void EndFrame()
    {
        CommandList.Close();
        CommandQueue.ExecuteCommandList(CommandList);

        var commandFrame = _commandFrames[FrameIndex];

        var fenceValue = ++_fenceValue;

        commandFrame.FenceValue = fenceValue;

        _ = CommandQueue.Signal(
            _fence,
            fenceValue);

        FrameIndex = (FrameIndex + 1) % Common.FRAME_BUFFER_COUNT;
    }

    public void FlushFrames()
    {
        for (var i = 0; i < Common.FRAME_BUFFER_COUNT; ++i)
        {
            _commandFrames[i].Wait(
                _fenceEvent,
                _fence);
        }
        FrameIndex = 0;
    }

    public void ReleaseFrames()
    {
        foreach (var commandFrame in _commandFrames)
        {
            commandFrame.Dispose();
        }
    }

    public void Dispose()
    {
        FlushFrames();

        _fence.Dispose();
        //_fenceValues = [];

        _fenceEvent.Dispose();

        CommandQueue.Dispose();
        CommandList.Dispose();

        ReleaseFrames();
    }

    private sealed class CommandFrame(
        ILogger logger,
        ID3D12CommandAllocator allocator)
        : IDisposable
    {
        private readonly ILogger _logger = logger;
        private readonly ID3D12CommandAllocator _allocator = allocator;
        public ulong FenceValue { get; set; }

        public void Reset(
            in ID3D12GraphicsCommandList10 commandlist)
        {
            _allocator.Reset();
            commandlist.Reset(_allocator);
        }

        public void Wait(
            AutoResetEvent fenceEvent,
            ID3D12Fence1 fence)
        {
            if (fence.CompletedValue < FenceValue)
            {
                _ = fence.SetEventOnCompletion(
                    value: FenceValue,
                    waitHandle: fenceEvent);

                _ = fenceEvent.WaitOne();
            }
        }

        public void Dispose()
        {
            _allocator.Dispose();
            FenceValue = 0;
        }
    }
}