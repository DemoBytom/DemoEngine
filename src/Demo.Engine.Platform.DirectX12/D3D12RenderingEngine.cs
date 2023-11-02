// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX12;

public class D3D12RenderingEngine : IRenderingEngine
{
    private bool _disposedValue;
    private readonly ILogger<D3D12RenderingEngine> _logger;
    private readonly RenderSettings _renderSettings;

    public IRenderingControl Control { get; }

    public Matrix4x4 ViewProjectionMatrix => throw new NotImplementedException();

    public D3D12RenderingEngine(
        ILogger<D3D12RenderingEngine> logger,
        IRenderingControl renderingControl,
        IOptionsSnapshot<RenderSettings> renderSettings)
    {
        _logger = logger;
        Control = renderingControl;
        _renderSettings = renderSettings.Value;
        //Initialize D3D12

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
}