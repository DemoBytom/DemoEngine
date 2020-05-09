using System.Numerics;

namespace Demo.Engine.Core.Interfaces.Rendering
{
    public interface ICube : IDrawable
    {
        void Update(Vector3 position, float rotationAngleInRadians);
    }
}