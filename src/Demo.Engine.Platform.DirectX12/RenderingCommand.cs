// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12;

// TODO: Add to DI via KeyedServices
internal sealed class RenderingCommand
    : IDisposable
{
    public const ushort FRAME_BUFFER_COUNT = 3;

    public ID3D12CommandQueue CommandQueue { get; }

    public ID3D12GraphicsCommandList10 CommandList { get; }

    public int FrameIndex { get; private set; } = 0;

    private readonly CommandFrame[] _commandFrames;

    private readonly ID3D12Fence1 _fence;
    private ulong _fenceValue = 0;
    private readonly AutoResetEvent _fenceEvent;

    public RenderingCommand(
        ID3D12Device14 device,
        CommandListType commandListType)
    {
        CommandQueue = device.CreateCommandQueue(
            type: commandListType,
            priority: CommandQueuePriority.Normal,
            flags: CommandQueueFlags.None,
            nodeMask: 0);

        CommandQueue.Name = commandListType switch
        {
            CommandListType.Direct
                => "GFX Command Queue",
            CommandListType.Compute
                => "Compute Command Queue",
            _
                => "Command Queue",
        };

        CommandList = device.CreateCommandList1<ID3D12GraphicsCommandList10>(
            type: commandListType,
            commandListFlags: CommandListFlags.None);

        CommandList.Name = commandListType switch
        {
            CommandListType.Direct
                => "GFX Command List",
            CommandListType.Compute
                => "Compute Command List",
            _
                => "Command List",
        };

        _commandFrames = new CommandFrame[FRAME_BUFFER_COUNT];
        for (var i = 0; i < FRAME_BUFFER_COUNT; ++i)
        {
            var allocator = device.CreateCommandAllocator(
                commandListType);
            allocator.Name = commandListType switch
            {
                CommandListType.Direct
                    => $"GFX Command Allocator[{i}]",
                CommandListType.Compute
                    => $"Compute Command Allocator[{i}]",
                _
                    => $"Command Allocator[{i}]",
            };

            _commandFrames[i] = new CommandFrame(allocator);
        }

        _fence = device.CreateFence<ID3D12Fence1>(
            _fenceValue,
            FenceFlags.None);

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

        commandFrame.FenceValue = ++_fenceValue;

        _ = CommandQueue.Signal(
            _fence,
            _fenceValue);

        FrameIndex = ++FrameIndex % FRAME_BUFFER_COUNT;
    }

    public void FlushFrames()
    {
        for (var i = 0; i < FRAME_BUFFER_COUNT; ++i)
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
        ReleaseFrames();

        _fence.Dispose();
        CommandList.Dispose();
        CommandQueue.Dispose();
    }

    private sealed class CommandFrame(
        ID3D12CommandAllocator allocator)
        : IDisposable
    {
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