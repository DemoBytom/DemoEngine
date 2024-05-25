// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX12;

public class D3D12RenderingEngine : IRenderingEngine
{
    private const int FRAME_BUFFER_COUNT = 2;

    private bool _disposedValue;
    private readonly ILogger<D3D12RenderingEngine> _logger;
    private readonly IDebugLayerLogger _debugLayerLogger;
    private readonly RenderSettings _renderSettings;

    private readonly IDXGIFactory7 _dxgiFactory;

    private readonly ID3D12Fence1 _fence;
    private ulong _frameCount;
    private readonly AutoResetEvent _fenceEvent;

    private readonly ID3D12CommandAllocator _commandAllocator;
    private readonly ID3D12GraphicsCommandList7 _commandList;

    private readonly IDXGISwapChain3 _swapChain;

    public IRenderingControl Control { get; }

    public Matrix4x4 ViewProjectionMatrix => throw new NotImplementedException();

    public RawBool IsTearingSupported { get; }

    public ID3D12Device10 Device { get; }
    public ID3D12CommandQueue CommandQueue { get; }

    public D3D12RenderingEngine(
        ILogger<D3D12RenderingEngine> logger,
        IRenderingControl renderingControl,
        IOptionsSnapshot<RenderSettings> renderSettings,
        IDebugLayerLogger debugLayerLogger)
    {
        _logger = logger;
        Control = renderingControl;
        _debugLayerLogger = debugLayerLogger;
        _renderSettings = renderSettings.Value;
        const bool DEBUG = false;

        //Initialize D3D12

        if (!D3D12.IsSupported(FeatureLevel.Level_12_0))
        {
            throw new PlatformNotSupportedException(
                "DirectX 12 is not supported on this machine!");
        }

        _dxgiFactory = DXGI.CreateDXGIFactory2<IDXGIFactory7>(
            debug: DEBUG);

        ID3D12Device10? d3d12Device = default;

        for (uint adapterIndex = 0;
            _dxgiFactory
                .EnumAdapters1(
                    adapterIndex,
                    out var adapter)
                .Success;
            ++adapterIndex)
        {
            var desc = adapter.Description1;

            // don't select the basic renderer driver adapter
            if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
            {
                adapter.Dispose();
                continue;
            }

            if (D3D12
                .D3D12CreateDevice(
                    adapter: adapter,
                    minFeatureLevel: FeatureLevel.Level_11_0,
                    device: out d3d12Device)
                .Success)
            {
                adapter.Dispose();
                break;
            }
        }

        using (var factory5 = _dxgiFactory.QueryInterfaceOrNull<IDXGIFactory5>())
        {
            if (factory5 is not null)
            {
                IsTearingSupported = factory5.PresentAllowTearing;
            }
        }

        if (d3d12Device is null)
        {
            throw new PlatformNotSupportedException("Cannot create ID3D12Device!");
        }

        Device = d3d12Device;

        CommandQueue = Device.CreateCommandQueue(
          type: CommandListType.Direct,
          priority: CommandQueuePriority.High,
          flags: CommandQueueFlags.None,
          nodeMask: 0);
        CommandQueue.Name = "Graphics Queue";

        _fence = Device.CreateFence<ID3D12Fence1>(
            initialValue: _frameCount,
            flags: FenceFlags.None);
        _fenceEvent = new AutoResetEvent(false);

        _commandAllocator = Device.CreateCommandAllocator<ID3D12CommandAllocator>(
            type: CommandListType.Direct);
        _commandList = Device.CreateCommandList1<ID3D12GraphicsCommandList7>(
            type: CommandListType.Direct,
            commandListFlags: CommandListFlags.None);

        using (var swapChain = _dxgiFactory.CreateSwapChainForHwnd(
            CommandQueue,
            Control.Handle,
            desc: new SwapChainDescription1
            {
                Width = (uint)_renderSettings.Width,
                Height = (uint)_renderSettings.Height,
                Format = Format.R8G8B8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription
                {
                    Count = 1,
                    Quality = 0,
                },
                BufferUsage = Usage.Backbuffer
                            | Usage.RenderTargetOutput,
                BufferCount = FRAME_BUFFER_COUNT,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore,
                Flags = SwapChainFlags.AllowModeSwitch
                      | SwapChainFlags.AllowTearing,
            },
            fullscreenDesc: new SwapChainFullscreenDescription
            {
                Windowed = true,
            }))
        {
            _ = _dxgiFactory.MakeWindowAssociation(
                Control.Handle,
                WindowAssociationFlags.IgnoreAltEnter);

            _swapChain = swapChain.QueryInterface<IDXGISwapChain3>();
        }

        //Show controll:
        Control.Show();
    }

    public void BeginScene(Color4 color)
    {
    }

    public void BeginScene()
        => BeginScene(new Color4(red: 0, green: 0, blue: 0, alpha: 1));

    public void Draw(IEnumerable<IDrawable> drawables)
    {
        InitCommandList();

        ExecutedCommandList();
    }

    public bool EndScene()
    {
        var result = _swapChain.Present(1, PresentFlags.None);

        return !result.Failure
            || result.Code != Vortice.DXGI.ResultCode.DeviceRemoved.Code;
    }

    private void SignalAndWait()
    {
        _ = CommandQueue.Signal(
            fence: _fence,
            value: ++_frameCount);

        _ = _fence.SetEventOnCompletion(
            value: _frameCount,
            waitHandle: _fenceEvent);

        _ = _fenceEvent.WaitOne();
    }

    private void Flush(int count)
    {
        for (var i = 0; i < count; ++i)
        {
            SignalAndWait();
        }
    }

    private void InitCommandList()
    {
        _commandAllocator.Reset();
        _commandList.Reset(_commandAllocator);
    }

    private void ExecutedCommandList()
    {
        _commandList.Close();

        CommandQueue.ExecuteCommandList(_commandList);

        SignalAndWait();
    }

    protected void Disposing(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                //Dispose managed resources

                //Flush all buffers that might be waiting in the queue
                Flush(FRAME_BUFFER_COUNT);

                _ = _swapChain.GetFullscreenState(out var fullscreenState);
                if (fullscreenState == true)
                {
                    _ = _swapChain.SetFullscreenState(false);
                }
                _swapChain.Dispose();

                _commandList.Dispose();
                _commandAllocator.Dispose();
                _fenceEvent.Dispose();
                _fence.Dispose();
                CommandQueue.Dispose();
                Device.Dispose();
                _dxgiFactory.Dispose();
            }
            //free unmanaged resources

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Disposing(true);
        GC.SuppressFinalize(this);
    }

    public void LogDebugMessages()
        => _debugLayerLogger.LogMessages();
}