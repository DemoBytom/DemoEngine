using System;
using System.Numerics;
using Demo.Engine.Core.Interfaces.Platform;
using Vortice.Mathematics;

namespace Demo.Engine.Core.Interfaces.Rendering
{
    public interface IRenderingEngine : IDisposable
    {
        IRenderingControl Control { get; }

        void BeginScene(Color4 color);

        bool EndScene();

        void BeginScene();

        void DrawCube(Vector3 position, float rotationAngleInRadians);
    }
}