// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.ValueObjects;
using Demo.Engine.Platform.DirectX12.RenderingResources;
using Microsoft.Extensions.DependencyInjection;
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
    private bool _disposedValue;
    private readonly ILogger<D3D12RenderingEngine> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDebugLayerLogger _debugLayerLogger;
    private readonly RenderSettings _renderSettings;

    private readonly IDXGIFactory7 _dxgiFactory;

    private readonly RenderingCommand _d3d12Command;
    private readonly bool[] _deferredReleasesFlag = new bool[Common.FRAME_BUFFER_COUNT];
    private readonly List<IDisposable>[] _deferredReleases;

    private readonly Lock _lock = new();

    public RTVDescriptorHeapAllocator RTVHeapAllocator { get; }
    public DSVDescriptorHeapAllocator DSVHeapAllocator { get; }
    public SRVDescriptorHeapAllocator SRVHeapAllocator { get; }
    public UAVDescriptorHeapAllocator UAVHeapAllocator { get; }

    private readonly ConcurrentDictionary<RenderingSurfaceId, RenderingSurface> _surfaces = [];

    public IReadOnlyCollection<IRenderingSurface> RenderingSurfaces
        => (IReadOnlyCollection<IRenderingSurface>)_surfaces.Values;

    public bool TryGetRenderingSurface(
        RenderingSurfaceId renderingSurfaceId,
        [NotNullWhen(true)]
        out IRenderingSurface? renderingSurface)
    {
        var gotSuccessfully = _surfaces.TryGetValue(renderingSurfaceId, out var outValue);
        renderingSurface = outValue;

        return gotSuccessfully;
    }

    public RawBool IsTearingSupported { get; }

    public ID3D12Device14 Device { get; }
    public ID3D12CommandQueue CommandQueue => _d3d12Command.CommandQueue;
    public ID3D12GraphicsCommandList10 CommandList => _d3d12Command.CommandList;

    public D3D12RenderingEngine(
        ILogger<D3D12RenderingEngine> logger,
        IServiceProvider serviceProvider,
        IOptionsSnapshot<RenderSettings> renderSettings,
        IDebugLayerLogger debugLayerLogger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _debugLayerLogger = debugLayerLogger;
        _renderSettings = renderSettings.Value;
        const bool DEBUG = false;

        _deferredReleases = new List<IDisposable>[Common.FRAME_BUFFER_COUNT];
        for (var i = 0; i < _deferredReleases.Length; ++i)
        {
            _deferredReleases[i] = [];
        }

        //Initialize D3D12

        if (!D3D12.IsSupported(FeatureLevel.Level_12_0))
        {
            throw new PlatformNotSupportedException(
                "DirectX 12 is not supported on this machine!");
        }

        _dxgiFactory = DXGI.CreateDXGIFactory2<IDXGIFactory7>(
            debug: DEBUG);
        _dxgiFactory.NameObject("Main DXGI Factory", _logger);

        IsTearingSupported = _dxgiFactory.PresentAllowTearing;

        Device = CreateDevice<ID3D12Device14>(_dxgiFactory);

        _d3d12Command = new RenderingCommand(
            serviceProvider.GetRequiredService<ILogger<RenderingCommand>>(),
            Device,
            CommandListType.Direct);

        //Create RTV Heap
        this
            .CreateDescriptorHeaps()
            .RTV(capacity: 512, isShaderVisible: false, out var rtv)
            .DSV(capacity: 512, isShaderVisible: false, out var dsv)
            .SRV(capacity: 4096, isShaderVisible: true, out var srv)
            .UAV(capacity: 512, isShaderVisible: false, out var uav)
            .VerifyAllDescriptorsCreatedProperly()
            ;

        RTVHeapAllocator = rtv;
        DSVHeapAllocator = dsv;
        SRVHeapAllocator = srv;
        UAVHeapAllocator = uav;

        //renderingSurface = new RenderingSurface(
        //    serviceProvider);

        //_surfaces.Add(renderingSurface.ID, renderingSurface);

        //Show controll:
        //foreach (var surface in _surfaces.Values)
        //{
        //    surface.CreateSwapChain(
        //        factory: _dxgiFactory,
        //        device: Device,
        //        commandQueue: _d3d12Command.CommandQueue,
        //        rtvDescriptorHeapAllocator: RTVHeapAllocator);
        //    surface.RenderingControl.Show();
        //}
    }

    public RenderingSurfaceId CreateSurface()
    {
        var renderingSurface = new RenderingSurface(
            _serviceProvider);

        renderingSurface.CreateSwapChain(
                factory: _dxgiFactory,
                device: Device,
                commandQueue: _d3d12Command.CommandQueue,
                rtvDescriptorHeapAllocator: RTVHeapAllocator);
        renderingSurface.RenderingControl.Show();

        _ = _surfaces.TryAdd(renderingSurface.ID, renderingSurface);

        return renderingSurface.ID;
    }

    private TAdapter CreateAdapter<TAdapter, TDXGIFactory>(
        TDXGIFactory dxgiFactory,
        FeatureLevel minimumFeatureLevel,
        out FeatureLevel maxSupportedFeatureLevel)
        where TAdapter : IDXGIAdapter
        where TDXGIFactory : IDXGIFactory6
    {
        for (uint adapterIndex = 0;
            dxgiFactory.EnumAdapterByGpuPreference(
                index: adapterIndex,
                gpuPreference: GpuPreference.HighPerformance,
                adapter: out TAdapter? adapter)
            .Success(in adapter);
            ++adapterIndex)
        {
            adapter.NameObject($"adapter[{adapterIndex}]_1", _logger);
            if (adapter is IDXGIAdapter1 adapter1
                // don't select the basic renderer driver adapter
                && (adapter1.Description1.Flags & AdapterFlags.Software) != AdapterFlags.None)
            {
                adapter.Dispose();
                continue;
            }

            if (D3D12
                .D3D12CreateDevice<ID3D12Device>(
                    adapter: adapter,
                    minFeatureLevel: minimumFeatureLevel,
                    device: out var device)
                .Success(
                    in device))
            {
                device.NameObject("Temp device for Feature Support", _logger);
                maxSupportedFeatureLevel = device.CheckMaxSupportedFeatureLevel();
                device.Dispose();

                return adapter;
            }
        }

        throw new PlatformNotSupportedException("No suitable adapter found!");
    }

    private TDevice CreateDevice<TDevice>(
        IDXGIFactory7 dxgiFactory,
        FeatureLevel minimumFeatureLevel = FeatureLevel.Level_11_0)
        where TDevice : ID3D12Device
    {
        using (var adapter = CreateAdapter<IDXGIAdapter4, IDXGIFactory7>(
            dxgiFactory,
            minimumFeatureLevel,
            out var maxSupportedFeatureLevel))
        {
            if (D3D12
                .D3D12CreateDevice(
                    adapter: adapter,
                    minFeatureLevel: maxSupportedFeatureLevel,
                    device: out TDevice? device)
                .Success(in device))
            {
                device.NameObject("Main device", _logger);
                _logger.LogDebug("Created device supporting {featureLevel}", maxSupportedFeatureLevel);
                return device;
            }
        }
        throw new PlatformNotSupportedException("Cannot create ID3D12Device!");
    }

    private Color4 _clearColor = new(red: 0.0f, green: 0.0f, blue: 0.0f, alpha: 1.0f);

    public uint CurrentFrameIndex()
        => _d3d12Command.FrameIndex;

    public void SetDeferredReleasesFlag()
        => _deferredReleasesFlag[CurrentFrameIndex()] = true;

    private void BeginFrame(
        in ID3D12Resource backBuffer,
        CpuDescriptorHandle rtv)
    {
        var barrier = ResourceBarrier.BarrierTransition(
            resource: backBuffer,
            stateBefore: ResourceStates.Present,
            stateAfter: ResourceStates.RenderTarget,
            subresource: 0,
            flags: ResourceBarrierFlags.None);

        _d3d12Command.CommandList.ResourceBarrier(barrier);

        //draw to render target?
        _d3d12Command.CommandList.ClearRenderTargetView(
            rtv,
            _clearColor);

        _d3d12Command.CommandList.OMSetRenderTargets(
            renderTargetDescriptor: rtv,
            depthStencilDescriptor: null);
    }

    private void EndFrame(
        ID3D12Resource backBuffer)
    {
        var barrier = ResourceBarrier.BarrierTransition(
            resource: backBuffer,
            stateBefore: ResourceStates.RenderTarget,
            stateAfter: ResourceStates.Present,
            subresource: 0,
            flags: ResourceBarrierFlags.None);

        _d3d12Command.CommandList.ResourceBarrier(barrier);
    }

    private void ProcessDeferredReleases(uint frameIndex)
    {
        lock (_lock)
        {
            _deferredReleasesFlag[frameIndex] = false;

            RTVHeapAllocator.ProcessDeferredFree(frameIndex);
            DSVHeapAllocator.ProcessDeferredFree(frameIndex);
            SRVHeapAllocator.ProcessDeferredFree(frameIndex);
            UAVHeapAllocator.ProcessDeferredFree(frameIndex);

            var disposables = _deferredReleases[frameIndex];

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            disposables.Clear();
        }
    }

    public void DeferredRelease(IDisposable disposable)
    {
        var frameIndex = CurrentFrameIndex();
        lock (_lock)
        {
            _deferredReleases[frameIndex].Add(disposable);
            SetDeferredReleasesFlag();
        }
    }

    public void Draw(
        RenderingSurfaceId renderingSurfaceId,
        IEnumerable<IDrawable> drawables)
        => Draw(
            _clearColor,
            renderingSurfaceId,
            drawables);

    public void Draw(
        Color4 color,
        RenderingSurfaceId renderingSurfaceId,
        IEnumerable<IDrawable> drawables)
    {
        InitCommandList();

        var frameIndex = CurrentFrameIndex();
        if (!_surfaces
            .TryGetValue(
                renderingSurfaceId,
                out var renderingSurface)
            || !renderingSurface.IsValid)
        {
            throw new InvalidOperationException(
                "Rendering surface is not valid!");
        }
        _clearColor = color;
        if (_deferredReleasesFlag[frameIndex])
        {
            ProcessDeferredReleases(frameIndex);
        }

        _ = renderingSurface.Present();

        // recrod commands
        BeginFrame(
            renderingSurface.BackBuffer,
            renderingSurface.RTV.Value);

        //TODO draw
        foreach (var drawable in drawables)
        {
            drawable.Draw(renderingSurface);
        }

        EndFrame(renderingSurface.BackBuffer);

        ExecuteCommandList();
    }

    public bool EndScene(
        RenderingSurfaceId renderingSurfaceId)
        => !_surfaces.TryGetValue(renderingSurfaceId, out var renderingSurface)
        || !renderingSurface.IsValid
            ? throw new InvalidOperationException(
                "Rendering surface is not valid!")
            : renderingSurface.Present();

    public void SetFullscreen(bool isFullscreen)
    {
        //if (isFullscreen != Control.IsFullscreen)
        //{
        //    Control.SetFullscreen(isFullscreen);
        //}
    }

    private void Resize(
        Width width,
        Height height)
    {
        //Debug.WriteLine($"Resizing SC from {_swapChainWidth}x{_swapChainHeight} to {width}x{height}");

        //ReleaseBuffers();

        //FlushBuffers();
        //_ = _swapChain.ResizeBuffers(
        //    bufferCount: RenderingCommand.FRAME_BUFFER_COUNT,
        //    width: (uint)width.Value,
        //    height: (uint)height.Value,
        //    newFormat: Format.Unknown,
        //    swapChainFlags: SwapChainFlags.AllowModeSwitch
        //                  | SwapChainFlags.AllowTearing);

        //_swapChainWidth = width.Value;
        //_swapChainHeight = height.Value;

        //_ = GetBuffers();
    }

    private void FlushBuffers()
        => _d3d12Command.FlushFrames();

    public void InitCommandList()
        => _d3d12Command.BeginFrame();

    public void ExecuteCommandList()
        => _d3d12Command.EndFrame();

    public T GetRequiredService<T>()
        where T : notnull
        => _serviceProvider.GetRequiredService<T>();

    protected void Disposing(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                try
                {
                    // Must be released before resources that depend on those (like a swap chain)
                    for (
                        uint frameIndex = 0;
                        frameIndex < Common.FRAME_BUFFER_COUNT;
                        ++frameIndex)
                    {
                        ProcessDeferredReleases(frameIndex);
                    }

                    //Dispose managed resources
                    // Not sure if I need to do
                    //    the deferred releasing here..

                    RTVHeapAllocator.DeferredRelease();
                    DSVHeapAllocator.DeferredRelease();
                    SRVHeapAllocator.DeferredRelease();
                    UAVHeapAllocator.DeferredRelease();

                    //Flush all buffers that might be waiting in the queue
                    FlushBuffers();
                    _d3d12Command.Dispose();

                    foreach (var renderingSurface in _surfaces.Values)
                    {
                        renderingSurface.Dispose();
                    }

                    // make sure everything is actually released
                    for (
                        uint frameIndex = 0;
                        frameIndex < Common.FRAME_BUFFER_COUNT;
                        ++frameIndex)
                    {
                        ProcessDeferredReleases(frameIndex);
                    }

                    RTVHeapAllocator.Dispose();
                    DSVHeapAllocator.Dispose();
                    SRVHeapAllocator.Dispose();
                    UAVHeapAllocator.Dispose();

                    Device.Dispose();
                    _dxgiFactory.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Error disposing Rendering Engine!");
                }
                finally
                {
                    _debugLayerLogger.LogMessages();
                }
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