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
    private bool _disposedValue;
    private readonly ILogger<D3D12RenderingEngine> _logger;
    private readonly IDebugLayerLogger _debugLayerLogger;
    private readonly RenderSettings _renderSettings;
    private readonly ID3D12Device10? _d3d12Device;

    public IRenderingControl Control { get; }

    public Matrix4x4 ViewProjectionMatrix => throw new NotImplementedException();

    public RawBool IsTearingSupported { get; }

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
            throw new InvalidOperationException(
                "DirectX 12 is not supported on this machine!");
        }

        var dxgiFactory = DXGI.CreateDXGIFactory2<IDXGIFactory4>(
            debug: DEBUG);

        _d3d12Device = default;

        for (var adapterIndex = 0;
            dxgiFactory.EnumAdapters1(adapterIndex, out var adapter).Success;
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
                    device: out _d3d12Device)
                .Success)
            {
                adapter.Dispose();
                break;
            }
        }

        using (var factory5 = dxgiFactory.QueryInterfaceOrNull<IDXGIFactory5>())
        {
            if (factory5 is not null)
            {
                IsTearingSupported = factory5.PresentAllowTearing;
            }
        }

        //Show controll:
        Control.Show();
    }

    public void BeginScene(Color4 color)
    {
    }

    public void BeginScene()
    {
    }

    public void Draw(IEnumerable<IDrawable> drawables)
    {
    }

    public bool EndScene()
        => true;

    protected void Disposing(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                //Dispose managed resources
                //var refCount = _d3d12Device?.Release();
                //_d3d12Device?.Dispose();
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