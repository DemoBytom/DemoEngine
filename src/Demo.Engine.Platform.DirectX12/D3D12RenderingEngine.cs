// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Numerics;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX12;

internal class D3D12RenderingEngine : ID3D12RenderingEngine
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

    private readonly IDXGISwapChain3 _swapChain;
    private readonly ID3D12Resource2[] _buffers = new ID3D12Resource2[FRAME_BUFFER_COUNT];
    private uint _currentBufferIndex = 0;

    private readonly ID3D12DescriptorHeap _renderTargetViewDescriptorHeap;
    private readonly CpuDescriptorHandle[] _rendertargetViewHandles = new CpuDescriptorHandle[FRAME_BUFFER_COUNT];

    public IRenderingControl Control { get; }

    private const int FOV = 90;
    private const float FOV_RAD = FOV * (MathF.PI / 180);

    public Matrix4x4 ViewProjectionMatrix
        => Matrix4x4.Transpose(
            // View matrix - Camera
            Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 4.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY)
            // Projection matrix - perspective
            //* Matrix4x4.CreatePerspective(1, Control.DrawHeight / (float)Control.DrawWidth, 0.1f, 10f));
            * Matrix4x4.CreatePerspectiveFieldOfView(
                FOV_RAD,
                (float)Control.DrawWidth.Value / Control.DrawHeight.Value,
                0.1f,
                10f));

    public RawBool IsTearingSupported { get; }

    public ID3D12Device10 Device { get; }
    public ID3D12CommandQueue CommandQueue { get; }
    public ID3D12GraphicsCommandList7 CommandList { get; }

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
        CommandList = Device.CreateCommandList1<ID3D12GraphicsCommandList7>(
            type: CommandListType.Direct,
            commandListFlags: CommandListFlags.None);

        _swapChainWidth = _renderSettings.Width;
        _swapChainHeight = _renderSettings.Height;
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
        //Create RTV Heap
        var result = Device.CreateDescriptorHeap(
            new DescriptorHeapDescription
            {
                Type = DescriptorHeapType.RenderTargetView,
                DescriptorCount = FRAME_BUFFER_COUNT,
                Flags = DescriptorHeapFlags.None,
                NodeMask = 0
            },
            out var rtvd);

        if (result
            is { Failure: true }
            || rtvd is null)
        {
            throw new InvalidOperationException("Error creating render target view");
        }

        _renderTargetViewDescriptorHeap = rtvd;

        var firstHandle = _renderTargetViewDescriptorHeap.GetCPUDescriptorHandleForHeapStart();

        var handleIncrement = Device.GetDescriptorHandleIncrementSize(
            DescriptorHeapType.RenderTargetView);

        for (var i = 0;
            i < FRAME_BUFFER_COUNT;
            ++i)
        {
            _rendertargetViewHandles[i] = new CpuDescriptorHandle(
                other: firstHandle,
                offsetInDescriptors: i,
                descriptorIncrementSize: handleIncrement);
        }

        // get buffers
        if (!GetBuffers())
        {
            throw new InvalidOperationException("Error getting buffers!");
        }

        //Show controll:
        Control.Show();
    }

    private Color4 _clearColor = new(red: 0.0f, green: 0.0f, blue: 0.0f, alpha: 1.0f);

    public void BeginScene(Color4 color)
    {
        // Check if resize is required
        if ((_swapChainWidth != Control.DrawWidth
            || _swapChainHeight != Control.DrawHeight)
            && Control.DrawWidth != 0
            && Control.DrawHeight != 0)
        {
            Resize(Control.DrawWidth, Control.DrawHeight);
        }

        _clearColor = color;
    }

    public void BeginScene()
        => BeginScene(new Color4(red: 0, green: 0, blue: 0, alpha: 1));

    private void BeginFrame()
    {
        _currentBufferIndex = _swapChain.CurrentBackBufferIndex;

        var barrier = ResourceBarrier.BarrierTransition(
            resource: _buffers[_currentBufferIndex],
            stateBefore: ResourceStates.Present,
            stateAfter: ResourceStates.RenderTarget,
            subresource: 0,
            flags: ResourceBarrierFlags.None);

        CommandList.ResourceBarrier(barrier);

        //draw to render target?
        CommandList.ClearRenderTargetView(
            _rendertargetViewHandles[_currentBufferIndex],
            _clearColor);

        CommandList.OMSetRenderTargets(
            renderTargetDescriptor: _rendertargetViewHandles[_currentBufferIndex],
            depthStencilDescriptor: null);
    }

    private void EndFrame()
    {
        var barrier = ResourceBarrier.BarrierTransition(
            resource: _buffers[_currentBufferIndex],
            stateBefore: ResourceStates.RenderTarget,
            stateAfter: ResourceStates.Present,
            subresource: 0,
            flags: ResourceBarrierFlags.None);

        CommandList.ResourceBarrier(barrier);
    }

    public void Draw(IEnumerable<IDrawable> drawables)
    {
        InitCommandList();
        BeginFrame();
        //TODO draw
        foreach (var drawable in drawables)
        {
            drawable.Draw();
        }

        EndFrame();

        ExecutedCommandList();
    }

    public bool EndScene()
    {
        var result = _swapChain.Present(1, PresentFlags.None);

        return !result.Failure
            || result.Code != Vortice.DXGI.ResultCode.DeviceRemoved.Code;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        if (isFullscreen != Control.IsFullscreen)
        {
            Control.SetFullscreen(isFullscreen);
        }
    }

    private Width _swapChainWidth = 0;
    private Height _swapChainHeight = 0;

    private void Resize(
        Width width,
        Height height)
    {
        Debug.WriteLine($"Resizing SC from {_swapChainWidth}x{_swapChainHeight} to {width}x{height}");

        ReleaseBuffers();

        FlushBuffers();
        _ = _swapChain.ResizeBuffers(
            bufferCount: FRAME_BUFFER_COUNT,
            width: (uint)width.Value,
            height: (uint)height.Value,
            newFormat: Format.Unknown,
            swapChainFlags: SwapChainFlags.AllowModeSwitch
                          | SwapChainFlags.AllowTearing);

        _swapChainWidth = width.Value;
        _swapChainHeight = height.Value;

        _ = GetBuffers();
    }

    private bool GetBuffers()
    {
        for (var i = 0; i < FRAME_BUFFER_COUNT; ++i)
        {
            if (!_swapChain.GetBuffer<ID3D12Resource2>((uint)i, out var buffer).Success)
            {
                return false;
            }

            _buffers[i] = buffer!;

            Device.CreateRenderTargetView(
                _buffers[i],
                new RenderTargetViewDescription
                {
                    Format = Format.R8G8B8A8_UNorm,
                    ViewDimension = RenderTargetViewDimension.Texture2D,
                    Texture2D =
                    {
                        MipSlice = 0,
                        PlaneSlice = 0,
                    },
                },
                _rendertargetViewHandles[i]);
        }

        return true;
    }

    private void ReleaseBuffers()
    {
        for (var i = 0; i < FRAME_BUFFER_COUNT; ++i)
        {
            _buffers[i].Dispose();
        }
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

    private void FlushBuffers()
    {
        for (var i = 0; i < FRAME_BUFFER_COUNT; ++i)
        {
            SignalAndWait();
        }
    }

    public void InitCommandList()
    {
        _commandAllocator.Reset();
        CommandList.Reset(_commandAllocator);
    }

    public void ExecutedCommandList()
    {
        CommandList.Close();

        CommandQueue.ExecuteCommandList(CommandList);

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
                FlushBuffers();

                ReleaseBuffers();

                _renderTargetViewDescriptorHeap.Dispose();

                _ = _swapChain.GetFullscreenState(out var fullscreenState);
                if (fullscreenState == true)
                {
                    _ = _swapChain.SetFullscreenState(false);
                }
                _swapChain.Dispose();

                CommandList.Dispose();
                _commandAllocator.Dispose();
                _fenceEvent.Dispose();
                _fence.Dispose();
                CommandQueue.Dispose();
                Device.Dispose();
                _dxgiFactory.Dispose();
            }
            //free unmanaged resources

            _disposedValue = true;

            _logger.LogInformation("Destroyed engine!");
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