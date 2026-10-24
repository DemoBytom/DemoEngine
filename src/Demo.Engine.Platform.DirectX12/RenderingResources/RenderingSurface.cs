// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vortice;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

internal sealed class RenderingSurface
    : IAsyncDisposable
    , IRenderingSurface
{
    private readonly EventHandler<RenderingControlSizeEventArgs> _resizeFunc;
    private readonly EventHandler<EventArgs> _pauseRenderingFunc;
    private readonly EventHandler<EventArgs> _resumeRenderingFunc;

    public RenderingSurface(
        IServiceProvider serviceProvider,
        Action flushBuffersFunc)
    {
        _scope = serviceProvider.CreateAsyncScope();
        _logger = _scope.ServiceProvider.GetRequiredService<ILogger<RenderingSurface>>();
        RenderingControl = _scope.ServiceProvider.GetRequiredService<IRenderingControl>();
        _flushBuffers = flushBuffersFunc;

        _resizeFunc = (sender, eventArgs) =>
        {
            if (eventArgs.Width > 0 && eventArgs.Height > 0
                && (eventArgs.Width != Width || eventArgs.Height != Height))
            {
                using var activity = InstrumentationDX12.ActivitySource.StartActivity("RenderingSurface.Resize", ActivityKind.Internal);

                Resize();

                _logger.LogResizedRenderingSurface(
                    surfaceId: ID,
                    width: eventArgs.Width,
                    height: eventArgs.Height);
            }
        };

        _pauseRenderingFunc = (sender, eventArgs) =>
        {
            _logger.LogRenderingPaused(ID);
            RenderingPaused = true;
        };

        _resumeRenderingFunc = (sender, eventArgs) =>
        {
            _logger.LogRenderingResumed(ID);
            RenderingPaused = false;
        };

        RenderingControl.UserResized += _resizeFunc;
        RenderingControl.PauseRendering += _pauseRenderingFunc;
        RenderingControl.ResumeRendering += _resumeRenderingFunc;
    }

    public RenderingSurfaceId ID { get; } = RenderingSurfaceId.NewId();
    public IRenderingControl RenderingControl { get; }

    private readonly Action _flushBuffers;
    private IDXGISwapChain4? _swapChain;
    private uint _currentBackBuffer;
    private Format _format = Common.DEFAULT_BACK_BUFFER_FORMAT;
    private bool _allowTearing;
    private PresentFlags _presentFlags;
    private bool _disposedValue;
    private readonly RenderTargetData[] _renderTargetDatas = new RenderTargetData[Common.BACK_BUFFER_COUNT];
    private readonly ILogger<RenderingSurface> _logger;
    private bool _createdSwapChain = false;
    private Func<ID3D12Device14>? _getDevice;
    private readonly AsyncServiceScope _scope;

    public Viewport Viewport { get; private set; }
    public RawRect ScissorRect { get; private set; }
    public Width Width => (int)Viewport.Width;
    public Height Height => (int)Viewport.Height;
    public ID3D12Resource? BackBuffer => _renderTargetDatas[_currentBackBuffer].Resource;

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public bool RenderingPaused { get; private set; }

    public WaitScope EnterLock() => new(_semaphoreSlim);

    public readonly ref struct WaitScope
        : IDisposable
    {
        public WaitScope(
            SemaphoreSlim semaphoreSlim)
        {
            _semaphoreSlim = semaphoreSlim;
            _semaphoreSlim.Wait();
        }

#pragma warning disable CA2213 // Disposable fields should be disposed
        private readonly SemaphoreSlim _semaphoreSlim;
#pragma warning restore CA2213 // Disposable fields should be disposed

        public void Dispose()
        {
            _semaphoreSlim.Release();
        }
    }

    [MemberNotNullWhen(true, nameof(BackBuffer))]
    [MemberNotNullWhen(true, nameof(RTV))]
    public bool IsValid
        => _renderTargetDatas[_currentBackBuffer].Resource is not null
        && _renderTargetDatas[_currentBackBuffer].RTV.IsValid;

    public CpuDescriptorHandle? RTV => _renderTargetDatas[_currentBackBuffer].RTV.CPU;

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
                (float)RenderingControl.DrawWidth.Value / RenderingControl.DrawHeight.Value,
                0.1f,
                10f));

    public async ValueTask CreateSwapChainAsync(
        IDXGIFactory7 factory,
        Func<ID3D12Device14> getDevice,
        ID3D12CommandQueue commandQueue,
        RTVDescriptorHeapAllocator rtvDescriptorHeapAllocator,
        Format format = Common.DEFAULT_BACK_BUFFER_FORMAT)
    {
        _getDevice = getDevice;
        var device = getDevice();
        //Release old resources if the method is called 2nd time
        if (_createdSwapChain)
        {
            await DisposeAsync();
            _disposedValue = false;
            _createdSwapChain = false;
        }

        _format = format;
        _allowTearing = factory.PresentAllowTearing == true;
        //_allowTearing = false;
        if (_allowTearing)
        {
            _presentFlags = PresentFlags.AllowTearing;
        }

        var swapChaingDescription = new SwapChainDescription1(
            width: (uint)RenderingControl.DrawWidth.Value,
            height: (uint)RenderingControl.DrawHeight.Value,
            format: ToNonSrgb(format),
            stereo: false,
            bufferUsage: Usage.Backbuffer
                       | Usage.RenderTargetOutput,
            bufferCount: Common.BACK_BUFFER_COUNT,
            scaling: Scaling.Stretch,
            swapEffect: SwapEffect.FlipDiscard,
            alphaMode: AlphaMode.Unspecified,
            flags: _allowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None);

        using (var swapChain = factory.CreateSwapChainForHwnd(
            commandQueue,
            RenderingControl.Handle,
            swapChaingDescription,
            fullscreenDesc: new SwapChainFullscreenDescription
            {
                Windowed = true,
            }))
        {
            _ = factory.MakeWindowAssociation(
                RenderingControl.Handle,
                WindowAssociationFlags.IgnoreAltEnter);
            swapChain.NameObject("TMP SwapChain");

            _swapChain = swapChain.QueryInterface<IDXGISwapChain4>();
            _swapChain.NameObject($"Main SwapChain ID: {ID}", _logger);
        }

        _currentBackBuffer = _swapChain.CurrentBackBufferIndex;

        for (var i = 0; i < _renderTargetDatas.Length; ++i)
        {
            _renderTargetDatas[i] = new RenderTargetData(
                rtvDescriptorHeapAllocator.Allocate());
        }

        GetBuffers(device);
        _createdSwapChain = true;
    }

    public void GetBuffers(
        ID3D12Device14 device)
    {
        if (_swapChain is null)
        {
            throw new InvalidOperationException(
                "Swap chain does not exist!");
        }

        for (var i = 0; i < _renderTargetDatas.Length; ++i)
        {
            if (_renderTargetDatas[i].Resource is not null)
            {
                throw new InvalidOperationException(
                    "Render target resource should have been null!");
            }

            var data = _renderTargetDatas[i];
            if (!data.RTV.IsValid)
            {
                throw new InvalidOperationException(
                    "Invalid RTV Descriptor!");
            }
            var resource = _swapChain.GetBuffer<ID3D12Resource2>((uint)i);
            var name = $"Buffer[{i}] res {resource.NativePointer}";
            resource.NameObject(name, _logger);

            _renderTargetDatas[i] = data with { Resource = resource };

            var rtvDescription = new RenderTargetViewDescription
            {
                Format = _format,
                ViewDimension = RenderTargetViewDimension.Texture2D,
                Texture2D =
                {
                    MipSlice = 0,
                    PlaneSlice = 0,
                },
            };

            device.CreateRenderTargetView(
                resource,
                rtvDescription,
                _renderTargetDatas[i].RTV.CPU);
        }

        var swapChainDesc = _swapChain.Description;
        if (swapChainDesc.BufferDescription.Width != RenderingControl.DrawWidth.Value
            || swapChainDesc.BufferDescription.Height != RenderingControl.DrawHeight.Value)
        {
            throw new InvalidOperationException("SwapChain and window sizes don't match!");
        }

        var drawingArea = RenderingControl.DrawingArea;

        Viewport = new Viewport(
            x: drawingArea.X,
            y: drawingArea.Y,
            width: drawingArea.Width,
            height: drawingArea.Height,
            minDepth: 1.0f,
            maxDepth: 0.0f);

        ScissorRect = new RawRect(
            left: 0,
            top: 0,
            right: RenderingControl.DrawWidth.Value,
            bottom: RenderingControl.DrawHeight.Value);
    }

    private static Format ToNonSrgb(Format format)
        => format switch
        {
            Format.R8G8B8A8_UNorm_SRgb => Format.R8G8B8A8_UNorm,
            _ => format,
        };

    public bool Present()
    {
        if (_swapChain is null)
        {
            throw new InvalidOperationException(
                "Swap chain does not exist!");
        }
        // TODO Present( 0.. - zdaje się nie synchronizować z odświeżaniem monitora i głupieć
        //var result = _swapChain.Present(0, _presentFlags);
        var result = _swapChain.Present(
            _allowTearing ? (uint)0 : 1,
            _presentFlags);

        if (result.Failure
            || result.Code == Vortice.DXGI.ResultCode.DeviceRemoved.Code)
        {
            return false;
        }

        _currentBackBuffer = _swapChain.CurrentBackBufferIndex;
        return true;

    }

    public void Resize()
    {
        using var lockScope = EnterLock();
        _flushBuffers();
        if (_swapChain is null)
        {
            throw new InvalidOperationException(
                "Swap chain does not exist!");
        }

        for (var i = 0; i < _renderTargetDatas.Length; ++i)
        {
            _renderTargetDatas[i].Resource?.Dispose();
            _renderTargetDatas[i] = _renderTargetDatas[i] with { Resource = null };
        }

        var flags = _allowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None;

        try
        {
            _ = _swapChain.ResizeBuffers(
                bufferCount: Common.BACK_BUFFER_COUNT,
                // using 0 for width and height to let the swap chain use the current window size
                width: 0,
                height: 0,
                // unknown format will keep the current format that was specified when swapchain was created
                newFormat: Format.Unknown,
                swapChainFlags: flags);
        }
        catch (SEHException ex)
        {
            _logger.LogError(ex, "Access violation during swap chain resize.");
            throw;
        }
        _currentBackBuffer = _swapChain.CurrentBackBufferIndex;

        GetBuffers(_getDevice!.Invoke());
    }

    private readonly record struct RenderTargetData
    {
        public ID3D12Resource? Resource { get; init; }

        /// <summary>
        /// Render Target View
        /// </summary>
        public DescriptorHandle<RTVDescriptorHeapAllocator> RTV { get; }

        public RenderTargetData()
          => throw new InvalidOperationException(
              "Render target data cannot be created without proper descriptor handles!");

        public RenderTargetData(
            DescriptorHandle<RTVDescriptorHeapAllocator> rtv)
            => RTV = rtv;
    }

    private async ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                RenderingControl.UserResized -= _resizeFunc;
                RenderingControl.PauseRendering -= _pauseRenderingFunc;
                RenderingControl.ResumeRendering -= _resumeRenderingFunc;

                _logger.LogDisposingRenderingSurface(ID);
                for (var rtvId = 0; rtvId < _renderTargetDatas.Length; ++rtvId)
                {
                    _logger.LogDisposingRTV(rtvId);

                    _ = _renderTargetDatas[rtvId].Resource?.Name;

                    if (_renderTargetDatas[rtvId].RTV.IsValid)
                    {
                        _renderTargetDatas[rtvId].RTV.Dispose();
                    }

                    _renderTargetDatas[rtvId].Resource?.Dispose();
                    _renderTargetDatas[rtvId] = _renderTargetDatas[rtvId] with { Resource = null };
                    _logger.LogDisposedRTV(rtvId);
                }

                _swapChain?.Dispose();
                _swapChain = null;
                _logger.LogDisposedRenderingSurface(ID);

                /* TODO: Dispose is also called when CreateSwapChain is called 2nd time
                 * this will dispose this scope and cause problems
                 * when accessing the rendering form!! */
                await _scope.DisposeAsync();

                _semaphoreSlim.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~RenderingSurface()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public async ValueTask DisposeAsync()
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        => await DisposeAsync(disposing: true);
    //GC.SuppressFinalize(this);
}