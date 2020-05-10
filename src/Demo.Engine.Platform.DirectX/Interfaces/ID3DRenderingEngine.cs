using Demo.Engine.Core.Interfaces.Rendering;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Interfaces
{
    public interface ID3DRenderingEngine : IRenderingEngine
    {
        public ID3D11Device Device { get; }
        public ID3D11DeviceContext DeviceContext { get; }
    }
}