using Demo.Engine.Platform.DirectX.Interfaces;

namespace Demo.Engine.Platform.DirectX.Bindable
{
    /// <summary>
    /// Vertex shader constant buffer that can be bound to the renrering pipeline
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VSConstantBuffer<T> : ConstantBuffer<T>
        where T : unmanaged
    {
        public VSConstantBuffer(
            ID3DRenderingEngine renderingEngine,
            ref T data)
            : base(
                renderingEngine,
                ref data)
        {
        }

        public override void Bind() => _renderingEngine.DeviceContext
            .VSSetConstantBuffer(0, _constantBuffer);
    }
}