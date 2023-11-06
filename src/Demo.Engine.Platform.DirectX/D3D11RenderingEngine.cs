// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Platform.DirectX.Interfaces;
using Demo.Tools.Common.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX;

public class D3D11RenderingEngine : ID3D11RenderingEngine
{
    private readonly ID3D11Texture2D _backBuffer;
    private readonly IDXGIFactory1 _factory;
    private readonly FeatureLevel _featureLevel;

    private readonly ILogger<D3D11RenderingEngine> _logger;
    private readonly ID3D11RenderTargetView _renderTargetView;
    private readonly ID3D11DepthStencilView _depthStencilView;
    private readonly ID3D11DepthStencilState _depthStencilState;
    private readonly ID3D11Texture2D _depthStencilTexture;
    private readonly IDXGISwapChain _swapChain;
    private readonly IOptionsMonitor<RenderSettings> _formSettings;
    private readonly IDebugLayerLogger _debugLayerLogger;

    private bool _disposedValue = false;

    public ID3D11Device Device { get; }
    public ID3D11DeviceContext DeviceContext { get; }

    public D3D11RenderingEngine(
        ILogger<D3D11RenderingEngine> logger,
        IRenderingControl renderingForm,
        IOptionsMonitor<RenderSettings> renderSettings,
        IDebugLayerLogger debugLayerLogger)
    {
        using var loggingContext = logger.LogScopeInitialization();
        _logger = logger;
        _formSettings = renderSettings;

        _debugLayerLogger = debugLayerLogger;

        Control = renderingForm;

        _factory = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () =>
            {
                DXGI.CreateDXGIFactory1(out IDXGIFactory1? factory)
                    .CheckError();

                return factory ?? throw new InvalidOperationException(
                    $"Cannot create {nameof(IDXGIFactory1)} instance!");
            });

        var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
        deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
        (Device, _featureLevel, DeviceContext) = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () =>
            {
                D3D11.D3D11CreateDevice(
                    adapterPtr: IntPtr.Zero,
                    driverType: DriverType.Hardware,
                    flags: deviceCreationFlags,
                    featureLevels: new[]
                    {
                            FeatureLevel.Level_11_1,
                            FeatureLevel.Level_11_0
                    },
                    device: out var device,
                    featureLevel: out var featureLevel,
                    immediateContext: out var deviceContext
                    )
                .CheckError();

                return (device, featureLevel, deviceContext);
            });

        _logger.LogDebug("Initiated device with {featureLeve}", _featureLevel);

        var isTearingSupported = false;
        using (var factory5 = _factory.QueryInterfaceOrNull<IDXGIFactory5>())
        {
            if (factory5 is not null)
            {
                isTearingSupported = factory5.PresentAllowTearing;
            }
        }

        var swapChainDescription = new SwapChainDescription
        {
            BufferCount = 2,
            BufferDescription = new ModeDescription(
                (uint)_formSettings.CurrentValue.Width,
                (uint)_formSettings.CurrentValue.Height,
                Format.B8G8R8A8_UNorm),
            Windowed = true,
            OutputWindow = Control.Handle,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.FlipDiscard,
            BufferUsage = Usage.RenderTargetOutput,
            Flags = isTearingSupported
                ? SwapChainFlags.AllowTearing
                : SwapChainFlags.None,
        };

        _swapChain = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => _factory.CreateSwapChain(Device, swapChainDescription));

        _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => _factory.MakeWindowAssociation(Control.Handle, WindowAssociationFlags.IgnoreAltEnter));

        _backBuffer = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () =>
            {
                var sc = _swapChain.GetBuffer<ID3D11Texture2D>(0);
                sc.DebugName = "Back buffer";
                return sc;
            });

        _renderTargetView = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => Device.CreateRenderTargetView(_backBuffer));

        var rd = new RasterizerDescription
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            FrontCounterClockwise = true,
            DepthBias = 0,
            SlopeScaledDepthBias = 0.0f,
            DepthBiasClamp = 0.0f,
            DepthClipEnable = true,
            ScissorEnable = false,
            MultisampleEnable = false,
            AntialiasedLineEnable = false,
        };
        _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => DeviceContext.RSSetState(Device.CreateRasterizerState(rd)));

        _depthStencilState = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => Device.CreateDepthStencilState(
                new DepthStencilDescription
                {
                    DepthEnable = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthFunc = ComparisonFunction.Less
                }));

        _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => DeviceContext.OMSetDepthStencilState(_depthStencilState));

        _depthStencilTexture = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => Device.CreateTexture2D(
                new Texture2DDescription
                {
                    Width = (uint)_formSettings.CurrentValue.Width,
                    Height = (uint)_formSettings.CurrentValue.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.D32_Float,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil
                }));

        _depthStencilView = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => Device.CreateDepthStencilView(
                _depthStencilTexture,
                new DepthStencilViewDescription
                {
                    Format = Format.D32_Float,
                    ViewDimension = DepthStencilViewDimension.Texture2D,
                    Texture2D = new Texture2DDepthStencilView { MipSlice = 0 }
                }));

        Control.Show();
    }

    public IRenderingControl Control { get; }

    private const int FOV = 90;
    private const float FOV_RAD = FOV * (MathF.PI / 180);

    /* TODO separate into view and projection matrices, and move View matrix to a Camera
     * Projection can (should be?) calculated once, and left there till it needs to be changed
     * */

    public Matrix4x4 ViewProjectionMatrix
        => Matrix4x4.Transpose(
            // View matrix - Camera
            Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 4.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY)
            // Projection matrix - perspective
            //* Matrix4x4.CreatePerspective(1, Control.DrawHeight / (float)Control.DrawWidth, 0.1f, 10f));
            * Matrix4x4.CreatePerspectiveFieldOfView(
                FOV_RAD,
                (float)Control.DrawWidth / Control.DrawHeight,
                0.1f,
                10f));

    public void BeginScene() => BeginScene(new Color4(0, 0, 0, 1));

    public void BeginScene(Color4 color)
        => _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () =>
            {
                DeviceContext.ClearRenderTargetView(_renderTargetView, color);
                DeviceContext.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            });

    public bool EndScene()
    {
        var result = _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () => _swapChain.Present(
                (uint)(_formSettings.CurrentValue.VSync
                    ? 1 : 0),
                PresentFlags.None));

        return !result.Failure
            || result.Code != Vortice.DXGI.ResultCode.DeviceRemoved.Code;
    }

    public void Draw(IEnumerable<IDrawable> drawables) =>
        //bind render target
        _debugLayerLogger.WrapCallInMessageExceptionHandler(
            () =>
            {
                DeviceContext.OMSetRenderTargets(_renderTargetView, _depthStencilView);
                foreach (var drawable in drawables)
                {
                    drawable.Draw();
                }
            });

    #region IDisposable Support

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).

                _renderTargetView.Dispose();
                _depthStencilView.Dispose();
                _depthStencilState.Dispose();
                _depthStencilTexture.Dispose();
                _backBuffer.Dispose();
                DeviceContext.ClearState();
                DeviceContext.Flush();
                DeviceContext.Dispose();
                Device.Dispose();
                _ = _swapChain.GetFullscreenState(out var fullscreen);
                if (fullscreen == true)
                {
                    _ = _swapChain.SetFullscreenState(false);
                }

                _swapChain.Dispose();
                _factory.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposedValue = true;
        }
    }

    public void LogDebugMessages()
    {
    }

    #endregion IDisposable Support
}